using System.Reflection;
using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers;

public static class TransformersLookup
{
    private static IEnumerable<TransformerMetadata> Transformers { get; } = Generate();

    public static TransformerMetadata Get(string name)
    {
        var transformer = Transformers.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (transformer == null)
            throw new Exception($"Transformer {name} not found");
        return transformer;
    }

    private static IEnumerable<TransformerMetadata> Generate()
    {
        var transformers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(ITransformer)) && !x.IsAbstract)
            .ToList();

        var allActions = new List<TransformerMetadata>();
        foreach (var transformer in transformers)
        {
            var attribute = transformer.GetCustomAttribute<TransformerAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name ?? transformer.Name;
            var metadata = new TransformerMetadata(name, attribute.Description, transformer);
            allActions.Add(metadata);
        }

        return allActions;
    }
}