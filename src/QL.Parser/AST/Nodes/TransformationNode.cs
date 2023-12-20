namespace QL.Parser.AST.Nodes;

public class TransformationNode : QLNode
{
    public string Name { get; set; }
    public List<ArgumentNode>? Arguments { get; set; } = [];
}