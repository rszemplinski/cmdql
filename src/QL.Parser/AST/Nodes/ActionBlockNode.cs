using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("ActionBlockNode {ActionType}")]
public class ActionBlockNode : QLNode
{
    public ActionTypeNode ActionType { get; set; }
    public List<ContextBlockNode> ContextBlocks { get; set; } = [];
}