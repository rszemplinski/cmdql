namespace QL.Core.FieldTransforms;

public record FieldTransformMetadata(string Name, string? Description, Type Type)
{
    public IFieldTransform CreateTransformer()
    {
        var transformer = Activator.CreateInstance(Type);
        if (transformer is not IFieldTransform transformerInterface)
        {
            throw new InvalidOperationException($"Transformer {Name} does not implement ITransformer");
        }

        return transformerInterface;
    }
}