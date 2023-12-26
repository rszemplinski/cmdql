using System.Collections.Concurrent;
using System.Text.Json;
using QL.Parser.AST.Nodes;
using QLShell.Sessions;
using Serilog;

namespace QLShell.Contexts;

public class AppContext
{
    public ActionBlockNode QueryRoot { get; }
    public SessionManager SessionManager { get; }

    public AppContext(ActionBlockNode root)
    {
        QueryRoot = root;
        SessionManager = new SessionManager();
        BuildSessions();
    }

    public async Task ExecuteAsync()
    {
        var allSessions = SessionManager.Sessions.Values
            .Select(x => x.session).ToList();
        if (allSessions.Count == 0)
        {
            Log.Information("No sessions to execute");
            return;
        }

        Log.Information("Connecting to sessions...");
        var connectionTasks = allSessions.Select(session => session.ConnectAsync());
        await Task.WhenAll(connectionTasks);

        var result = new ConcurrentDictionary<string, object>();
        await Parallel.ForEachAsync(SessionManager.Sessions,
            async (sessionData, cancellationToken) =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                var (_, data) = sessionData;
                var (session, contextBlock) = data;
                Log.Information("Executing session: {0}", session);
                var context = new SessionContext(session, contextBlock.SelectionSet);
                var contextResult = await context.ExecuteAsync();
                result.TryAdd(session.Info.Alias, contextResult);
            });
        
        Log.Information("Disconnecting from sessions...");
        var disconnectionTasks = allSessions.Select(session => session.DisconnectAsync());
        await Task.WhenAll(disconnectionTasks);
    }

    private void BuildSessions()
    {
        foreach (var contextBlock in QueryRoot.ContextBlocks)
        {
            var sessionInfo = ExtractSessionInfo(contextBlock);
            SessionManager.AddSession(sessionInfo, contextBlock);
        }
    }

    private static ISession ExtractSessionInfo(ContextBlockNode node)
    {
        return node switch
        {
            RemoteContextBlockNode remote => new RemoteSession(ExtractRemoteSessionInfo(remote)),
            LocalContextBlockNode => new LocalSession(ExtractLocalSessionInfo()),
            _ => throw new NotImplementedException()
        };
    }

    private static SessionInfo ExtractRemoteSessionInfo(RemoteContextBlockNode node)
    {
        // Convert arguments to dictionary
        var args = node.Arguments.ToDictionary(
            arg => arg.Name.ToLower(),
            arg => arg.Value
        );

        if (args["host"] is not StringValueNode host)
            throw new ArgumentException("Missing host argument");

        var port = args.TryGetValue("port", out var value) ? ((IntValueNode)value).Value : 22;
        var alias = args.TryGetValue("alias", out value) ? ((StringValueNode)value).Value : host.Value;

        if (args["user"] is not StringValueNode user)
            user = new StringValueNode
            {
                Value = Environment.UserName
            };

        if (args.TryGetValue("password", out var passwordValue))
        {
            if (passwordValue is not StringValueNode password)
                throw new ArgumentException("Missing password argument /or keyfile argument");

            return SessionInfo.CreateWithPassword(
                host.Value,
                user.Value,
                password.Value,
                alias,
                port
            );
        }

        var keyFile = args.TryGetValue("keyfile", out value)
            ? ((StringValueNode)value).Value
            : SshKeyFinder.FindDefaultSshPrivateKey();

        if (keyFile is null)
            throw new ArgumentException("Missing keyfile argument /or password argument");

        return SessionInfo.CreateWithKeyFile(
            host.Value,
            user.Value,
            keyFile,
            alias,
            port
        );
    }

    private static SessionInfo ExtractLocalSessionInfo()
        => SessionInfo.CreateLocalSessionInfo();
}