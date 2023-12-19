using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("{ActionType}")]
public class ActionTypeNode : QLNode
{
    public string ActionType { get; set; }
}