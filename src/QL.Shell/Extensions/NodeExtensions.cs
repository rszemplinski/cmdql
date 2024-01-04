using QL.Core;
using QL.Core.Actions;
using QL.Core.FieldTransforms;
using QL.FieldTransforms;
using QL.Parser.AST.Nodes;

namespace QLShell.Extensions;

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

    public static
        IReadOnlyDictionary<string, (FieldTransformMetadata transformer, IReadOnlyDictionary<string, object> arguments)[]>
        GetFieldsWithTransformers(this FieldNode field)
    {
        return field.SelectionSet?
            .Where(x => x.Field.Transformations.Any())
            .ToDictionary(
                x => x.Field.Name.ToCamelCase(),
                x => x.Field.Transformations
                    .Select(t => (FieldTransformsLookup.Get(t.Name),
                        t.Arguments?.BuildArgumentsDictionary() ?? new Dictionary<string, object>()))
                    .ToArray()
            ) ?? new Dictionary<string, (FieldTransformMetadata transformer, IReadOnlyDictionary<string, object> arguments)
            []>();
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