using System.Collections.Concurrent;
using QL.Actions;
using QL.Core;
using QL.Parser.AST.Nodes;
using QLShell.Sessions;

namespace QLShell.Contexts;

public class SessionContext(ISession session, IEnumerable<SelectionNode> selectionSet)
{
    private ISession Session { get; } = session;
    private IEnumerable<SelectionNode> SelectionSet { get; } = selectionSet;

    public async Task<IReadOnlyDictionary<string, object>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object>();

        var fields = SelectionSet
            .Select(x => x.Field)
            .ToList();

        var sessionSshClient = new SessionClient(Session);
        var executionTasks = fields.Select(async fieldNode =>
        {
            if (ActionsLookup.IsNamespace(fieldNode.Name))
            {
                var namespaceContext = new NamespaceContext(fieldNode.Name, Session.Platform, sessionSshClient,
                    fieldNode.SelectionSet!);
                var namespaceResult = await namespaceContext.ExecuteAsync(cancellationToken);
                result.TryAdd(fieldNode.Name, namespaceResult);
                return;
            }

            var actionContext = new ActionContext(Session.Platform, sessionSshClient, fieldNode);
            var actionResult = await actionContext.ExecuteAsync(cancellationToken);
            result.TryAdd(fieldNode.Name, actionResult);
        });
        
        await Task.WhenAll(executionTasks);

        return result;
    }

    private class SessionClient(ISession session) : IClient
    {
        private ISession Session { get; } = session;

        public Task<ICommandOutput> ExecuteCommandAsync(string command, CancellationToken cancellationToken)
        {
            return Session.ExecuteCommandAsync(command, cancellationToken);
        }

        public Task<ICommandOutput> UploadFileAsync(string localPath, string remotePath,
            CancellationToken cancellationToken = default)
        {
            return Session.UploadFileAsync(localPath, remotePath, cancellationToken);
        }

        public Task<ICommandOutput> DownloadFileAsync(string remotePath, string localPath,
            CancellationToken cancellationToken = default)
        {
            return Session.DownloadFileAsync(remotePath, localPath, cancellationToken);
        }
        
        public Task<bool> IsToolInstalledAsync(string toolName, CancellationToken cancellationToken = default)
        {
            return Session.IsToolInstalledAsync(toolName, cancellationToken);
        }

        public override string ToString()
        {
            return Session.ToString()!;
        }
    }
}