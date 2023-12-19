using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("SelectionNode {Field}")]
public class SelectionNode : QLNode
{
    public FieldNode Field { get; set; }
}