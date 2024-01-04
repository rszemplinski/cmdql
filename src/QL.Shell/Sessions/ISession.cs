using QL.Core;

namespace QLShell.Sessions;

public interface ISession
{
    bool IsConnected { get; }
    SessionInfo Info { get; }
    
    Platform Platform { get; }

    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken);

    Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
        CancellationToken cancellationToken = default);

    Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
        CancellationToken cancellationToken = default);
    
    Task<bool> IsToolInstalledAsync(string toolName, CancellationToken cancellationToken = default);

    Task DisconnectAsync(CancellationToken cancellationToken = default);
}