using QL.Actions;
using QL.Core;
using QL.Core.Exceptions;
using QL.Engine.Extensions;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Contexts;

public class ActionContext(Platform platform, IClient client, FieldNode fieldNode, string @namespace = "")
{
    private Platform Platform { get; } = platform;
    private IClient Client { get; } = client;
    private string Namespace { get; } = @namespace;

    public async Task<object> ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var action = ActionsLookup.Get(fieldNode.Name, Namespace).CreateAction(Platform);
            var arguments = fieldNode.BuildArgumentsDictionary();
            var allFields = fieldNode.GetFields();
            return await action.ExecuteCommandAsync(Client, arguments, allFields, cancellationToken);
        }
        catch (ActionException ex)
        {
            return new Dictionary<string, object>
            {
                { "error", ex.Message },
                { "exitCode", ex.ExitCode }
            };
        }
        catch (Exception ex)
        {
            return new Dictionary<string, object>
            {
                { "error", ex.Message },
                { "exitCode", 1 }
            };
        }
    }
}