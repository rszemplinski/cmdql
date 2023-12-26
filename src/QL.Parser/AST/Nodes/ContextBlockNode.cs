namespace QL.Parser.AST.Nodes;

public abstract class ContextBlockNode : QLNode
{
    public List<SelectionNode> SelectionSet { get; set; }
}

public class LocalContextBlockNode : ContextBlockNode;

public class RemoteContextBlockNode : ContextBlockNode
{
    public List<ArgumentNode> Arguments { get; set; }
}