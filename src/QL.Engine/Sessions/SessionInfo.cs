namespace QL.Engine.Sessions;

public readonly struct SessionInfo
{
    public bool IsUsingKeyFile => !string.IsNullOrEmpty(KeyFile);
    public bool IsUsingPassword => !string.IsNullOrEmpty(Password);
    
    public string Alias { get; private init; }
    public string Host { get; private init; }
    public string Username { get; private init; }
    public string? Password { get; private init; }
    public string KeyFile { get; private init; }
    public int Port { get; private init; }


    public static SessionInfo CreateWithPassword(string host, string username, string? password, string? alias = null, int port = 22)
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

    public static SessionInfo CreateLocalSessionInfo()
    {
        return new SessionInfo
        {
            Alias = "localhost",
            Host = "localhost",
            Username = Environment.UserName,
            Password = string.Empty,
            Port = 22
        };
    }
}