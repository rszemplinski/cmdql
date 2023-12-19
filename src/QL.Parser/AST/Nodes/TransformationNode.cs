namespace QL.Parser.AST.Nodes;

public class TransformationNode : QLNode
{
    public string Name { get; set; }
    public ArgumentsNode? Arguments { get; set; }
}