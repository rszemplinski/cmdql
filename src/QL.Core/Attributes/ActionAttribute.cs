namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ActionAttribute(string? name = null, string? description = null) : Attribute
{
    /**
     * A property that allows the ability to override the name of the <see cref="Actions.IAction"/>.
     */
    public string? Name { get; } = name;
    
    /**
     * A description of the <see cref="Actions.IAction"/>.
     */
    public string? Description { get; } = description;
}
