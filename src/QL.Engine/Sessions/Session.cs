using System.Diagnostics;
using System.Runtime.InteropServices;
using QL.Core;
using Renci.SshNet;
using Serilog;

namespace QL.Engine.Sessions;

public class LocalSession(SessionInfo info) : ISession
{
    public bool IsConnected { get; private set; }

    public SessionInfo Info { get; } = info;
    public Platform Platform => RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
            ? Platform.Linux
            : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                ? Platform.OSX
                : throw new PlatformNotSupportedException();
    
    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.CompletedTask;
    }

    public async Task<ICommandOutput> ExecuteCommandAsync(string command,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "sh",
            Arguments = $"-c \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        try
        {
            process.Start();
            
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            var waitForExitTask = process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(outputTask, errorTask, waitForExitTask);

            return new CommandOutput
            {
                Result = outputTask.Result,
                Error = errorTask.Result,
                ExitCode = process.ExitCode
            };
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while executing command: {0}", command);
            return new CommandOutput
            {
                Result = string.Empty,
                Error = string.Empty,
                ExitCode = -1
            };
        }
    }

    public Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Cannot upload file to local session");
    }

    public Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Cannot download file from local session");
    }
    
    public async Task<bool> IsToolInstalledAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteCommandAsync($"which {toolName}", cancellationToken);
        return result.ExitCode == 0;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public override string ToString()
    {
        // Check if alias is not the same as host
        var alias = Info.Alias == Info.Host ? string.Empty : $"({Info.Alias})";
        return string.IsNullOrEmpty(alias)
            ? $"{Info.Username}@{Info.Host}:{Info.Port}"
            : $"{alias} {Info.Username}@{Info.Host}:{Info.Port}";
    }
}

public class RemoteSession(SessionInfo info) : ISession
{
    public SessionInfo Info { get; } = info;
    public Platform Platform { get; private set; }
    public bool IsConnected => _client?.IsConnected ?? false;

    private SshClient? _client;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
            return;

        try
        {
            _client = Info.IsUsingKeyFile
                ? new SshClient(Info.Host, Info.Port, Info.Username, new PrivateKeyFile(Info.KeyFile))
                : new SshClient(Info.Host, Info.Port, Info.Username, Info.Password);

            await _client.ConnectAsync(cancellationToken);
            
            // Get platform
            var result = _client.RunCommand("uname");
            Platform = result.Result.StartsWith("Linux")
                ? Platform.Linux
                : result.Result.StartsWith("Darwin")
                    ? Platform.OSX
                    : throw new PlatformNotSupportedException();
            
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while connecting to session: {0}", this);
        }
    }

    public async Task<ICommandOutput> ExecuteCommandAsync(string command,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        try
        {
            var result = await Task.Run(() => _client?.RunCommand(command), cancellationToken);
            if (result == null)
                throw new InvalidOperationException("Something went wrong while executing command");

            return new CommandOutput
            {
                Result = result.Result,
                Error = result.Error,
                ExitCode = result.ExitStatus
            };
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while executing command: {0}", command);
            return new CommandOutput
            {
                Result = string.Empty,
                Error = string.Empty,
                ExitCode = -1
            };
        }
    }

    public async Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        var sftpClient = new SftpClient(_client!.ConnectionInfo);

        try
        {
            await sftpClient.ConnectAsync(cancellationToken);
            sftpClient.UploadFile(File.OpenRead(localPath), remotePath);

            return new CommandOutput
            {
                Result = $"Successfully uploaded file {localPath} to {remotePath}",
                Error = string.Empty,
                ExitCode = 0
            };
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while uploading file: {0}", localPath);
            return new CommandOutput
            {
                Result = string.Empty,
                Error = $"Unable to upload file {localPath} to {remotePath}",
                ExitCode = -1
            };
        }
        finally
        {
            sftpClient.Disconnect();
        }
    }

    public async Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");
        
        var sftpClient = new SftpClient(_client!.ConnectionInfo);
        try
        {
            await sftpClient.ConnectAsync(cancellationToken);
            var directory = Path.GetDirectoryName(localPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
           
            var fileStream = File.Create(localPath);
            sftpClient.DownloadFile(remotePath, fileStream);

            return new CommandOutput
            {
                Result = $"Successfully downloaded file {remotePath} to {localPath}",
                Error = string.Empty,
                ExitCode = 0
            };
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while downloading file: {0}", remotePath);
            return new CommandOutput
            {
                Result = string.Empty,
                Error = $"Unable to download file {remotePath} to {localPath}",
                ExitCode = -1
            };
        }
        finally
        {
            sftpClient.Disconnect();
        }
    }
    
    public async Task<bool> IsToolInstalledAsync(string toolName, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteCommandAsync($"which {toolName}", cancellationToken);
        return result.ExitCode == 0;
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            return;

        try
        {
            await Task.Run(() => _client?.Disconnect(), cancellationToken);
        }
        catch (TaskCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while disconnecting from session: {0}", this);
        }
    }

    public override string ToString()
    {
        // Check if alias is not the same as host
        var alias = Info.Alias == Info.Host ? string.Empty : $"({Info.Alias})";
        return string.IsNullOrEmpty(alias)
            ? $"{Info.Username}@{Info.Host}:{Info.Port}"
            : $"{alias} {Info.Username}@{Info.Host}:{Info.Port}";
    }
}