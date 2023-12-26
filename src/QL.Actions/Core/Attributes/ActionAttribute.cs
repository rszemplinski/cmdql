namespace QL.Actions.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ActionAttribute(string? name = null, string? description = null) : Attribute
{
    /**
     * A property that allows the ability to override the name of the action.
     */
    public string? Name { get; } = name;
    
    /**
     * A description of the action.
     */
    public string? Description { get; } = description;
}
