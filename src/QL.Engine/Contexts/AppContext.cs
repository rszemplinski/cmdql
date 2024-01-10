using System.Collections.Concurrent;
using System.Diagnostics;
using QL.Engine.Fields;
using QL.Engine.Sessions;
using QL.Engine.Utils;
using QL.Parser.AST.Nodes;
using Serilog;

namespace QL.Engine.Contexts;

public class AppContext
{
    private ActionBlockNode QueryRoot { get; }
    private AppConfig AppConfig { get; }
    private List<(ISession, ContextBlockNode)> Sessions { get; }

    public AppContext(ActionBlockNode root, AppConfig config)
    {
        QueryRoot = root;
        AppConfig = config;
        Sessions = BuildSessions();
    }

    public async Task<object> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object>();

        var sw = Stopwatch.StartNew();
        await Parallel.ForEachAsync(
            Sessions,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = AppConfig.MaxConcurrency,
            },
            async (data, token) =>
            {
                var (session, contextBlock) = data;
                Log.Debug("Connecting to {0}...", session);
                await session.ConnectAsync(token);

                if (!session.IsConnected)
                {
                    Log.Error("Failed to connect to {0}", session);
                    return;
                }
                
                var context = new SessionContext(session, contextBlock.SelectionSet);
                var contextResult = await context.ExecuteAsync(token);
                result.TryAdd(session.Info.Alias, contextResult);
                
                Log.Debug("Disconnecting from {0}...", session);
                await session.DisconnectAsync(token);
            });
        sw.Stop();
        Log.Debug("Executed all sessions in {0}ms", sw.ElapsedMilliseconds);
        
        sw.Restart();
        var fields = GetOrderedFields();
        var orderedResult = FieldProcessor.ProcessFields(fields, result);
        sw.Stop();
        Log.Debug("Processed fields in {0}ms", sw.ElapsedMilliseconds);

        return orderedResult;
    }

    private IEnumerable<Field> GetOrderedFields()
    {
        var fields = new List<Field>();
        foreach (var (session, contextBlock) in Sessions)
        {
            fields.Add(new Field(session.Info.Alias, contextBlock.SelectionSet));
        }
        return fields;
    }

    private List<(ISession, ContextBlockNode)> BuildSessions()
    {
        var sessions = new List<(ISession, ContextBlockNode)>();
        foreach (var contextBlock in QueryRoot.ContextBlocks)
        {
            var sessionInfo = ExtractSessionInfo(contextBlock);
            sessions.Add((sessionInfo, contextBlock));
        }
        return sessions;
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
        var user = (args.TryGetValue("user", out value) ? ((StringValueNode)value).Value : null) ?? Environment.UserName;

        if (args.TryGetValue("password", out var passwordValue))
        {
            if (passwordValue is not StringValueNode password)
                throw new ArgumentException("Missing password argument /or keyfile argument");

            return SessionInfo.CreateWithPassword(
                host.Value,
                user,
                password.Value,
                alias,
                port
            );
        }

        var keyFileStringValueNode = args.TryGetValue("keyfile", out value) ? value as StringValueNode : null;
        var keyFile = keyFileStringValueNode is not null
            ? PathResolver.GetFullPath(keyFileStringValueNode.Value)
            : SshKeyFinder.FindDefaultSshPrivateKey();

        if (keyFile is null)
            throw new ArgumentException("Missing keyfile argument /or password argument");

        return SessionInfo.CreateWithKeyFile(
            host.Value,
            user,
            keyFile,
            alias,
            port
        );
    }

    private static SessionInfo ExtractLocalSessionInfo()
        => SessionInfo.CreateLocalSessionInfo();
}