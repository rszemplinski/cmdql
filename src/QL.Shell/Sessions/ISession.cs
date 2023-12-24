namespace QLShell.Sessions;

public interface ISession : IAsyncDisposable
{
    SessionInfo Info { get; }
    
    Task ConnectAsync();
    Task<string> ExecuteCommandAsync(string command);
    Task DisconnectAsync();
}