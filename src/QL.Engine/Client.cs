using QL.Core;
using QL.Engine.Sessions;

namespace QL.Engine;

public class Client(ISession session) : IClient
{
    private ISession Session { get; } = session;
    
    public SessionType Type => Session is LocalSession
        ? SessionType.Local
        : SessionType.Remote;

    public Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
    {
        return Session.ExecuteCommandAsync(command, cancellationToken);
    }

    public Task<bool> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default)
    {
        return Session.UploadFileAsync(localPath, remotePath, cancellationToken);
    }
    
    public Task<bool> UploadFileAsync(FileStream fileStream, string remotePath,
        CancellationToken cancellationToken = default)
    {
        return Session.UploadFileAsync(fileStream, remotePath, cancellationToken);
    }

    public Task<FileStream?> DownloadFileAsync(string remotePath,
        CancellationToken cancellationToken = default)
    {
        return Session.DownloadFileAsync(remotePath, cancellationToken);
    }

    public Task<FileStream?> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default)
    {
        return Session.DownloadFileAsync(remotePath, localPath, cancellationToken);
    }

    public Task<bool> IsToolInstalledAsync(string toolName, CancellationToken cancellationToken = default)
    {
        return Session.IsToolInstalledAsync(toolName, cancellationToken);
    }

    public override string ToString()
    {
        return Session.ToString()!;
    }
}