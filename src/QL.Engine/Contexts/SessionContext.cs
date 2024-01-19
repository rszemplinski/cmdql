using System.Collections.Concurrent;
using QL.Engine.Lookups;
using QL.Engine.Sessions;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Contexts;

public class SessionContext(ISession session, IEnumerable<SelectionNode> selectionSet)
{
    private ISession Session { get; } = session;
    private IEnumerable<SelectionNode> SelectionSet { get; } = selectionSet;

    public async Task<IReadOnlyDictionary<string, object?>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object?>();

        var fields = SelectionSet
            .Select(x => x.Field)
            .ToList();

        var sessionSshClient = new Client(Session);
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
}