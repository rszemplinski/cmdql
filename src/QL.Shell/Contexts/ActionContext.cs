using QL.Actions;
using QL.Core;
using QL.Parser.AST.Nodes;
using QLShell.Extensions;
using QLShell.Utils;

namespace QLShell.Contexts;

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
            var result = await action.ExecuteCommandAsync(Client, arguments, allFields, cancellationToken);
            
            // TODO: A way to optimize this would be by only iterating over the fields that have transformers instead of all fields
            var fieldsWithTransformers = fieldNode.GetFieldsWithTransformers();
            var transformedResult = ObjectModifier.ModifyObject(result, (propName, value) =>
            {
                var propValue = value;
                if (fieldsWithTransformers.TryGetValue(propName, out var transformers))
                {
                    foreach (var (transformer, args) in transformers)
                    {
                        propValue = transformer.CreateTransformer().Apply(value, args);
                    }
                }
                return propValue;
            });
            
            return transformedResult;
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