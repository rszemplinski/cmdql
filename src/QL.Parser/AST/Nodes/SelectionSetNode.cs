using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("SelectionSetNode Selections: {Selections.Count}")]
public class SelectionSetNode : QLNode
{
    public List<SelectionNode> Selections { get; set; }
}