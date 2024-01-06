using QL.Core.Actions;
using QL.Core.FieldTransforms;
using QL.Engine.Extensions;
using QL.FieldTransforms;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Fields;

public class Field : IField
{
    public string Name { get; }

    public IEnumerable<(IFieldTransform fieldTransform, IReadOnlyDictionary<string, object> args)>
        Transformers { get; }

    public IField[] Fields { get; }

    public Field(FieldNode field)
    {
        Name = field.Name;
        Transformers = field.Transformations
            .Select(t =>
            (
                FieldTransformsLookup.Get(t.Name).CreateTransformer(),
                t.Arguments?.BuildArgumentsDictionary() ?? new Dictionary<string, object>()
            ))
            .ToArray();
        Fields = field.SelectionSet?
            .Select(x => new Field(x.Field) as IField)
            .ToArray() ?? [];
    }

    public Field(string name, IEnumerable<SelectionNode> selections)
    {
        Name = name;
        Fields = selections
            .Select(x => new Field(x.Field) as IField)
            .ToArray();
    }
}