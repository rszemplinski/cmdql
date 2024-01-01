namespace QL.Core.Transformers;

public record TransformerMetadata(string Name, string? Description, Type Type)
{
    public ITransformer CreateTransformer()
    {
        var transformer = Activator.CreateInstance(Type);
        if (transformer is not ITransformer transformerInterface)
        {
            throw new InvalidOperationException($"Transformer {Name} does not implement ITransformer");
        }

        return transformerInterface;
    }
}