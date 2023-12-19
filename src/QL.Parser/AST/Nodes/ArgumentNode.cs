using System.Diagnostics;

namespace QL.Parser.AST.Nodes;

[DebuggerDisplay("ArgumentNode {Name} = {Value}")]
public class ArgumentNode : QLNode
{
    public string Name { get; set; }
    public object Value { get; set; }
}