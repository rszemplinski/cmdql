using QL.Core.FieldTransforms;

namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TransformerAttribute : Attribute
{
    /**
     * A property that allows the ability to override the name of the <see cref="IFieldTransform"/>.
     */
    public string? Name { get; set; }
    
    /**
     * A description of the <see cref="IFieldTransform"/>.
     */
    public string? Description { get; set; }
}