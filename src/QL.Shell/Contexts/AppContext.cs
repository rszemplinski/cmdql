using System.Collections.Concurrent;
using System.Diagnostics;
using QL.Parser.AST.Nodes;
using QLShell.Sessions;
using QLShell.Utils;
using Serilog;

namespace QLShell.Contexts;

public class AppContext
{
    private ActionBlockNode QueryRoot { get; }
    private SessionManager SessionManager { get; }
    private AppConfig AppConfig { get; }

    public AppContext(ActionBlockNode root, AppConfig config)
    {
        QueryRoot = root;
        AppConfig = config;
        SessionManager = new SessionManager();
        BuildSessions();
    }

    public async Task<IReadOnlyDictionary<string, object>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object>();
        var sessions = SessionManager.Sessions
            .Select(x => x.Value);

        var sw = Stopwatch.StartNew();
        await Parallel.ForEachAsync(
            sessions,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = AppConfig.MaxConcurrency,
            },
            async (data, token) =>
            {
                var (session, contextBlock) = data;
                await session.ConnectAsync(token);
                var context = new SessionContext(session, contextBlock.SelectionSet);
                var contextResult = await context.ExecuteAsync(token);
                result.TryAdd(session.Info.Alias, contextResult);
                
                await session.DisconnectAsync(token);
            });
        sw.Stop();
        Log.Debug("Executed all sessions in {0}ms", sw.ElapsedMilliseconds);

        return result;
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