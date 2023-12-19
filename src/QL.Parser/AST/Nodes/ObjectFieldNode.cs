namespace QL.Parser.AST.Nodes;

public class ObjectFieldNode : QLNode
{
    public string Name { get; set; }
    public ValueNode Value { get; set; }
}