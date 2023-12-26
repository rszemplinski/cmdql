using QL.Parser.AST.Nodes;

namespace QLShell.Extensions;

public static class NodeExtensions
{
    public static Dictionary<string, object> BuildArgumentsDictionary(this FieldNode field)
    {
        return field.Arguments.ToDictionary(argument => argument.Name, argument => argument.Value);
    }
}