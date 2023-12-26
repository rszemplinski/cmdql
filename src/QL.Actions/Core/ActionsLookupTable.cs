using System.Reflection;
using QL.Actions.Core.Actions;
using QL.Actions.Core.Attributes;

namespace QL.Actions.Core;

public static class ActionsLookupTable
{
    private static Dictionary<string, ActionMetadata> LookupTable { get; } = Generate();
    
    public static ActionMetadata Get(string name)
    {
        name = name.ToLowerInvariant();
        if (!LookupTable.TryGetValue(name, out var metadata))
        {
            throw new InvalidOperationException($"Action {name} does not exist");
        }

        return metadata;
    }

    private static Dictionary<string, ActionMetadata> Generate()
    {
        var actions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IAction)) && !x.IsAbstract)
            .ToList();

        var lookupTable = new Dictionary<string, ActionMetadata>();
        foreach (var action in actions)
        {
            var attribute = action.GetCustomAttribute<ActionAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name ?? action.Name;
            var metadata = new ActionMetadata(name, attribute.Description, action);
            lookupTable.Add(name.ToLowerInvariant(), metadata);
        }

        return lookupTable;
    }
}