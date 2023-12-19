namespace QL.Parser.AST.Nodes;

public abstract class ContextBlockNode : QLNode
{
    
}

public class LocalContextBlockNode : ContextBlockNode
{
    public SelectionSetNode SelectionSet { get; set; }
}

public class RemoteContextBlockNode : ContextBlockNode
{
    public ArgumentsNode Arguments { get; set; }
    public SelectionSetNode SelectionSet { get; set; }
}