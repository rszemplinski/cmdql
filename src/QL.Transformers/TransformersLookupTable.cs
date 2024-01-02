using System.Collections.Concurrent;
using System.Reflection;
using QL.Core;
using QL.Core.Attributes;
using QL.Core.Transformers;

namespace QL.Transformers;

public static class TransformersLookupTable
{
    private static IReadOnlyDictionary<string, TransformerMetadata> LookupTable { get; } = Generate();
    
    public static TransformerMetadata Get(string name)
    {
        if (!LookupTable.TryGetValue(name.ToCamelCase(), out var metadata))
        {
            throw new InvalidOperationException($"Transformer {name} does not exist");
        }

        return metadata;
    }

    private static ConcurrentDictionary<string, TransformerMetadata> Generate()
    {
        var transformers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(ITransformer)) && !x.IsAbstract)
            .ToList();

        var lookupTable = new ConcurrentDictionary<string, TransformerMetadata>();
        foreach (var transformer in transformers)
        {
            var attribute = transformer.GetCustomAttribute<TransformerAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name ?? transformer.Name;
            var metadata = new TransformerMetadata(name, attribute.Description, transformer);
            lookupTable.TryAdd(name.ToCamelCase(), metadata);
        }

        return lookupTable;
    }
}