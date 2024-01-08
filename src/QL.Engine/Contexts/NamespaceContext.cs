using System.Collections.Concurrent;
using QL.Core;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Contexts;

public class NamespaceContext(
    string @namespace,
    Platform platform,
    IClient client,
    IEnumerable<SelectionNode> selectionSet)
{
    private string Namespace { get; } = @namespace;
    private Platform Platform { get; } = platform;
    private IClient Client { get; } = client;
    private IEnumerable<SelectionNode> SelectionSet { get; } = selectionSet;

    public async Task<IReadOnlyDictionary<string, object?>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object?>();

        var fields = SelectionSet
            .Select(x => x.Field)
            .ToList();
        
        var executionTasks = fields.Select(async fieldNode =>
        {
            var actionContext = new ActionContext(Platform, Client, fieldNode, Namespace);
            var actionResult = await actionContext.ExecuteAsync(cancellationToken);
            result.TryAdd(fieldNode.Name, actionResult);
        });

        await Task.WhenAll(executionTasks);

        return result;
    }
}