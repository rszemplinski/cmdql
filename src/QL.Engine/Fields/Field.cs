using QL.Core.Actions;
using QL.Parser.AST.Nodes;

namespace QL.Engine.Fields;

public class Field : IField
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
    
    public Field(string name, IEnumerable<SelectionNode> selections)
    {
        Name = name;
        Fields = selections
            .Select(x => new Field(x.Field) as IField)
            .ToArray();
    }
}