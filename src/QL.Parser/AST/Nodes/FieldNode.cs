using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("FieldNode {Name}")]
public class FieldNode : QLNode
{
    public string Name { get; set; }
    public ArgumentsNode? Arguments { get; set; }
    public TransformationsNode? Transformations { get; set; }
    public SelectionSetNode? SelectionSet { get; set; }
}