using System.Collections.Concurrent;
using QL.Actions;
using QL.Core;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;

namespace QLShell.Contexts;

public class NamespaceContext(
    string @namespace,
    Platform platform,
    IClient client,
    IEnumerable<SelectionNode> selectionSet)
{
    private Platform Platform { get; } = platform;
    private string Namespace { get; } = @namespace;
    private IClient Client { get; } = client;
    private IEnumerable<SelectionNode> SelectionSet { get; } = selectionSet;

    public async Task<IReadOnlyDictionary<string, object>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var result = new ConcurrentDictionary<string, object>();

        var fields = SelectionSet
            .Select(x => x.Field)
            .ToAsyncEnumerable();

        await foreach (var fieldNode in fields.WithCancellation(cancellationToken))
        {
            var action = ActionsLookup.Get(fieldNode.Name, Namespace).CreateAction(Platform);
            var arguments = fieldNode.BuildArgumentsDictionary();
            var allFields = fieldNode.GetSubFields();
            var response = await action
                .ExecuteCommandAsync(Client, arguments,
                    allFields, cancellationToken);
            result.TryAdd(fieldNode.Name, response);
        }

        return result;
    }
}