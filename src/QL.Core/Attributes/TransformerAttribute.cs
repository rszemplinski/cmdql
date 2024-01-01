namespace QL.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class TransformerAttribute(string? name = null, string? description = null) : Attribute
{
    /**
     * A property that allows the ability to override the name of the <see cref="Transformers.ITransformer"/>.
     */
    public string? Name { get; } = name;

    /**
     * A description of the <see cref="Transformers.ITransformer"/>.
     */
    public string? Description { get; } = description;
}