using System.Reflection;
using QL.Core.Attributes;
using QL.Core.FieldTransforms;

namespace QL.FieldTransforms;

public static class FieldTransformsLookup
{
    private static IEnumerable<FieldTransformMetadata> FieldTransforms { get; } = Generate();

    public static FieldTransformMetadata Get(string name)
    {
        var transformer = FieldTransforms.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (transformer == null)
            throw new Exception($"Transformer {name} not found");
        return transformer;
    }

    private static IEnumerable<FieldTransformMetadata> Generate()
    {
        var transformers = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IFieldTransform)) && !x.IsAbstract)
            .ToList();

        var allActions = new List<FieldTransformMetadata>();
        foreach (var transformer in transformers)
        {
            var attribute = transformer.GetCustomAttribute<TransformerAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name ?? transformer.Name;
            var metadata = new FieldTransformMetadata(name, attribute.Description, transformer);
            allActions.Add(metadata);
        }

        return allActions;
    }
}