using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using QL.Parser.AST.Nodes;
using QLShell.Sessions;
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
        var allSessions = SessionManager.Sessions.Values
            .Select(x => x.session).ToList();
        if (allSessions.Count == 0)
        {
            Log.Warning("There were no sessions to execute");
            return new Dictionary<string, object>();
        }

        var sw = Stopwatch.StartNew();
        var connectionTasks = allSessions.Select(session => session.ConnectAsync(cancellationToken));
        await Task.WhenAll(connectionTasks);
        sw.Stop();
        Log.Debug("Connected to {0} sessions in {1}ms", allSessions.Count, sw.ElapsedMilliseconds);

        sw.Restart();
        var result = new ConcurrentDictionary<string, object>();
        await Parallel.ForEachAsync(SessionManager.Sessions,
            new ParallelOptions
            {
                CancellationToken = cancellationToken
            },
            async (sessionData, cancelToken) =>
            {
                var (_, data) = sessionData;
                var (session, contextBlock) = data;
                var context = new SessionContext(session, contextBlock.SelectionSet);
                var contextResult = await context.ExecuteAsync(cancelToken);
                result.TryAdd(session.Info.Alias, contextResult);
            });
        sw.Stop();
        Log.Debug("Executed all sessions in {0}ms", sw.ElapsedMilliseconds);

        sw.Restart();
        var disconnectionTasks = allSessions.Select(session => session.DisconnectAsync(cancellationToken));
        await Task.WhenAll(disconnectionTasks);
        sw.Stop();
        Log.Debug("Disconnected from {0} sessions in {1}ms", allSessions.Count, sw.ElapsedMilliseconds);

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