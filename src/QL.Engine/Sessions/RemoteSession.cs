using QL.Core;
using Renci.SshNet;
using Serilog;

namespace QL.Engine.Sessions;

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
            var authMethods = new List<AuthenticationMethod>();
            if (Info is { IsUsingPassword: true, IsUsingKeyFile: true })
            {
                IPrivateKeySource[] privateKeyFiles =
                [
                    new PrivateKeyFile(Info.KeyFile, Info.Password)
                ];
                authMethods.Add(new PrivateKeyAuthenticationMethod(Info.Username, privateKeyFiles));
            }

            if (Info.IsUsingKeyFile)
            {
                IPrivateKeySource[] privateKeyFiles =
                [
                    new PrivateKeyFile(Info.KeyFile),
                ];
                authMethods.Add(new PrivateKeyAuthenticationMethod(Info.Username, privateKeyFiles));
            }

            if (Info.IsUsingPassword)
            {
                authMethods.Add(new PasswordAuthenticationMethod(Info.Username, Info.Password));
            }
            
            var connectionInfo = new ConnectionInfo(Info.Host, Info.Port, Info.Username, authMethods.ToArray());
            _client = new SshClient(connectionInfo);

            await _client.ConnectAsync(cancellationToken);

            var result = _client.RunCommand("uname");
            Platform = result.Result.StartsWith("Linux")
                ? Platform.Linux
                : result.Result.StartsWith("Darwin")
                    ? Platform.OSX
                    : throw new PlatformNotSupportedException();
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
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        using var sftpClient = new SftpClient(_client!.ConnectionInfo);

        try
        {
            await sftpClient.ConnectAsync(cancellationToken);
            sftpClient.UploadFile(File.OpenRead(localPath), remotePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while uploading file: {0}", localPath);
            return false;
        }
    }

    public async Task<bool> UploadFileAsync(FileStream fileStream, string remotePath,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        using var sftpClient = new SftpClient(_client!.ConnectionInfo);

        try
        {
            await sftpClient.ConnectAsync(cancellationToken);
            sftpClient.UploadFile(fileStream, remotePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while uploading file: {0}", fileStream.Name);
            return false;
        }
    }

    public async Task<FileStream?> DownloadFileAsync(string remotePath,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        using var sftpClient = new SftpClient(_client!.ConnectionInfo);

        try
        {
            await sftpClient.ConnectAsync(cancellationToken);

            var fileStream = File.Create(remotePath);
            sftpClient.DownloadFile(remotePath, fileStream);
            return new FileStream(remotePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while downloading file: {0}", remotePath);
            return null;
        }
    }

    public async Task<FileStream?> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Session is not connected");

        using var sftpClient = new SftpClient(_client!.ConnectionInfo);

        try
        {
            await sftpClient.ConnectAsync(cancellationToken);

            var fileStream = File.Create(localPath);
            sftpClient.DownloadFile(remotePath, fileStream);
            return new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while downloading file: {0}", remotePath);
            return null;
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