namespace QLShell.Sessions;

public abstract class Session(SessionInfo info) : ISession
{
    public bool IsConnected { get; protected set; }
    public SessionInfo Info { get; } = info;

    public abstract Task ConnectAsync();

    public abstract Task<string> ExecuteCommandAsync(string command);

    public abstract Task DisconnectAsync();
    
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
        GC.SuppressFinalize(this);
    }
}

public class LocalSession(SessionInfo info) : Session(info)
{
    public override Task ConnectAsync()
    {
        // Nothing to do here
        return Task.CompletedTask;
    }

    public override Task<string> ExecuteCommandAsync(string command)
    {
        throw new NotImplementedException();
    }

    public override Task DisconnectAsync()
    {
        // Nothing to do here
        return Task.CompletedTask;
    }
}

public class RemoteSession(SessionInfo info) : Session(info)
{
    public override Task ConnectAsync()
    {
        
        throw new NotImplementedException();
    }

    public override Task<string> ExecuteCommandAsync(string command)
    {
        throw new NotImplementedException();
    }

    public override Task DisconnectAsync()
    {
        throw new NotImplementedException();
    }
}