using System.Reflection;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions;

public static class ActionsLookup
{
    private static IEnumerable<ActionMetadata> Actions { get; } = Generate();

    public static ActionMetadata Get(string name, string @namespace = "")
    {
        var action = Actions.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            x.Namespace.Equals(@namespace, StringComparison.OrdinalIgnoreCase));
        if (action == null)
            throw new Exception($"Action {name} not found");
        return action;
    }

    public static bool IsNamespace(string ns)
    {
        return Actions.Any(x => x.Namespace == ns);
    }

    private static IEnumerable<ActionMetadata> Generate()
    {
        var actions = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsAssignableTo(typeof(IAction)) && !x.IsAbstract)
            .ToList();

        var allActions = new List<ActionMetadata>();
        foreach (var action in actions)
        {
            var attribute = action.GetCustomAttribute<ActionAttribute>();
            if (attribute == null)
                continue;

            var name = attribute.Name ?? action.Name;
            var ns = action.GetCustomAttribute<NamespaceAttribute>()?.Namespace;
            var metadata = new ActionMetadata
            {
                Name = name,
                Description = attribute.Description,
                Namespace = ns ?? string.Empty,
                Type = action,
            };
            allActions.Add(metadata);
        }

        return allActions;
    }
}