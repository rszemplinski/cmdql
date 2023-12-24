namespace QLShell.Sessions;

public struct SessionInfo
{
    public string Alias { get; init; }
    public string Host { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
    public string KeyFile { get; init; }
    public int Port { get; init; }


    public static SessionInfo CreateWithPassword(string host, string username, string password, string? alias = null, int port = 22)
    {
        return new SessionInfo
        {
            Alias = alias ?? host,
            Host = host,
            Username = username,
            Password = password,
            Port = port
        };
    }
    
    public static SessionInfo CreateWithKeyFile(string host, string username, string keyFile, string? alias = null, int port = 22)
    {
        return new SessionInfo
        {
            Alias = alias ?? host,
            Host = host,
            Username = username,
            KeyFile = keyFile,
            Port = port
        };
    }
    
    public static SessionInfo CreateWithKeyFile(string host, string username, string keyFile, string password, string? alias = null, int port = 22)
    {
        return new SessionInfo
        {
            Alias = alias ?? host,
            Host = host,
            Username = username,
            KeyFile = keyFile,
            Password = password,
            Port = port
        };
    }
}