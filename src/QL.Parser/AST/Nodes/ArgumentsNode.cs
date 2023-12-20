namespace QL.Parser.AST.Nodes;

public class ArgumentsNode : QLNode
{
    public List<ArgumentNode> Arguments { get; set; } = [];
}