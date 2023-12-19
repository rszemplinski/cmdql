namespace QL.Parser.AST.Nodes;

public abstract class ValueNode : QLNode { }

public class BooleanValueNode : ValueNode
{
    public bool Value { get; set; }
}

public class StringValueNode : ValueNode
{
    public string Value { get; set; } = string.Empty;
}

public class IntValueNode : ValueNode
{
    public int Value { get; set; }
}

public class DecimalValueNode : ValueNode
{
    public decimal Value { get; set; }
}

public class NullValueNode : ValueNode { }

public class ListValueNode : ValueNode
{
    public List<ValueNode> Values { get; set; } = [];
}

public class ObjectValueNode : ValueNode
{
    public List<ObjectFieldNode> Fields { get; set; } = [];
}

public class VariableValueNode : ValueNode
{
    public string Name { get; set; } = string.Empty;
}