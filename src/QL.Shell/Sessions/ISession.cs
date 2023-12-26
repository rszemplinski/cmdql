namespace QLShell.Sessions;

public interface ISession
{
    bool IsConnected { get; }
    SessionInfo Info { get; }
    
    Task ConnectAsync(CancellationToken cancellationToken = default);
    Task<string> ExecuteCommandAsync(string command);
    Task DisconnectAsync(CancellationToken cancellationToken = default);
}