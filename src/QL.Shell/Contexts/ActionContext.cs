using QL.Actions;
using QL.Core;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;

namespace QLShell.Contexts;

public class ActionContext(Platform platform, IClient client, FieldNode fieldNode)
{
    private Platform Platform { get; } = platform;
    private IClient Client { get; } = client;

    public async Task<object> ExecuteAsync(CancellationToken cancellationToken)
    {
        var action = ActionsLookup.Get(fieldNode.Name).CreateAction(Platform);
        var arguments = fieldNode.BuildArgumentsDictionary();
        var allFields = fieldNode.GetSubFields();
        var response = await action
            .ExecuteCommandAsync(Client, arguments,
                allFields, cancellationToken);
        return response;
    }
    
}