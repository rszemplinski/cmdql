using QL.Core;

namespace QLShell.Sessions;

public interface ISession
{
    bool IsConnected { get; }
    SessionInfo Info { get; }
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}