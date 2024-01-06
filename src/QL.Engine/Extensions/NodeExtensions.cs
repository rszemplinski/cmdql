using QL.Core.Actions;
using QL.Engine.Fields;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Extensions;

public static class NodeExtensions
{
    public static IReadOnlyDictionary<string, object> BuildArgumentsDictionary(this FieldNode field)
    {
        return field.Arguments.ToDictionary(argument => argument.Name, argument => argument.Value);
    }

    public static IReadOnlyDictionary<string, object> BuildArgumentsDictionary(this IEnumerable<ArgumentNode> arguments)
    {
        return arguments.ToDictionary(argument => argument.Name, argument => argument.Value);
    }

    public static IField[] GetFields(this FieldNode field)
    {
        return field.SelectionSet?
            .Select(x => new Field(x.Field) as IField)
            .ToArray() ?? [];
    }
}