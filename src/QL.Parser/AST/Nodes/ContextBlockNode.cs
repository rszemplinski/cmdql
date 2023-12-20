namespace QL.Parser.AST.Nodes;

public abstract class ContextBlockNode : QLNode
{
    
}

public class LocalContextBlockNode : ContextBlockNode
{
    public List<SelectionNode> SelectionSet { get; set; }
}

public class RemoteContextBlockNode : ContextBlockNode
{
    public List<ArgumentNode> Arguments { get; set; }
    public List<SelectionNode> SelectionSet { get; set; }
}