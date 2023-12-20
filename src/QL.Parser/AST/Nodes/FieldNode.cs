using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("FieldNode {Name}")]
public class FieldNode : QLNode
{
    public string Name { get; set; }
    public List<ArgumentNode> Arguments { get; set; } = [];
    public List<TransformationNode> Transformations { get; set; } = [];
    public List<SelectionNode>? SelectionSet { get; set; } = [];
}