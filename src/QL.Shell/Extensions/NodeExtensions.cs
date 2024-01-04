using QL.Core.Actions;
using QL.Parser.AST.Nodes;

namespace QLShell.Extensions;

public static class NodeExtensions
{
    public static IReadOnlyDictionary<string, object> BuildArgumentsDictionary(this FieldNode field)
    {
        return field.Arguments.ToDictionary(argument => argument.Name, argument => argument.Value);
    }
    
    public static IField[] GetSubFields(this FieldNode field)
    {
        return field.SelectionSet?
            .Select(x => new Field(x.Field) as IField)
            .ToArray() ?? [];
    }
    
    private class Field : IField
    {
        public string Name { get; }
        public IField[] Fields { get; }

        public Field(FieldNode field)
        {
            Name = field.Name;
            Fields = field.SelectionSet?
                .Select(x => new Field(x.Field) as IField)
                .ToArray() ?? [];
        }
    }
}