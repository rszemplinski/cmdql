namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TransformerAttribute : Attribute
{
    /**
     * A property that allows the ability to override the name of the <see cref="Transformers.ITransformer"/>.
     */
    public string? Name { get; set; }
    
    /**
     * A description of the <see cref="Transformers.ITransformer"/>.
     */
    public string? Description { get; set; }
}