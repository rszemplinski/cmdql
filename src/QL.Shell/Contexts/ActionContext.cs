using QL.Actions;
using QL.Core;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;
using Serilog;

namespace QLShell.Contexts;

public class ActionContext(Platform platform, IClient client, FieldNode fieldNode, string @namespace = "")
{
    private Platform Platform { get; } = platform;
    private IClient Client { get; } = client;

    public async Task<object> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var action = ActionsLookup.Get(fieldNode.Name, @namespace).CreateAction(Platform);
            var arguments = fieldNode.BuildArgumentsDictionary();
            var allFields = fieldNode.GetSubFields();
            return await action.ExecuteCommandAsync(Client, arguments, allFields, cancellationToken);
        }
        catch (Exception ex)
        {
            return new Dictionary<string, string>
            {
                { "error", ex.Message }
            };
        }
    }
}