using System.Diagnostics;
using System.Runtime.InteropServices;
using QL.Core;
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
        catch (Exception ex)
        {
            Log.Error(ex, "Error while executing command: {0}", command);
            return new CommandOutput
            {
                Result = string.Empty,
                Error = string.Empty,
                ExitCode = 1
            };
        }
    }

    public async Task<bool> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if source and destination are the same
            if (localPath.Equals(remotePath, StringComparison.OrdinalIgnoreCase))
                return true;

            // Perform the file copy
            await Task.Run(() => File.Copy(localPath, remotePath, overwrite: true), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while 'uploading' file in LocalSession: {0}", localPath);
            return false;
        }
    }

    public async Task<bool> UploadFileAsync(FileStream fileStream, string remotePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform the file copy
            await Task.Run(() => File.Copy(fileStream.Name, remotePath, overwrite: true), cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while 'uploading' file in LocalSession: {0}", fileStream.Name);
            return false;
        }
    }

    public Task<FileStream?> DownloadFileAsync(string remotePath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Since this is a local session, the remote path is actually a local path
            // We just open and return a FileStream to that path
            var fileStream = new FileStream(remotePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult<FileStream?>(fileStream);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while 'downloading' file in LocalSession: {0}", remotePath);
            return Task.FromResult<FileStream?>(null);
        }
    }

    public async Task<FileStream?> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Perform the file copy
            await Task.Run(() => File.Copy(remotePath, localPath, overwrite: true), cancellationToken);

            // Open and return the FileStream of the local copy
            var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return fileStream;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while 'downloading' file in LocalSession: {0}", remotePath);
            return null;
        }
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