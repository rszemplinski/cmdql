namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ActionAttribute : Attribute
{
    /**
     * A property that allows the ability to override the name of the <see cref="Actions.IAction"/>.
     */
    public string? Name { get; set; }
    
    /**
     * A description of the <see cref="Actions.IAction"/>.
     */
    public string? Description { get; set; }
}
