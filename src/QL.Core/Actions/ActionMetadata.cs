namespace QL.Core.Actions;

public record ActionMetadata
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public string Namespace { get; init; } = "";
    public Type Type { get; init; }

    public IAction CreateAction(Platform platform)
    {
        var action = Activator.CreateInstance(Type);
        if (action is not IAction actionInterface)
        {
            throw new InvalidOperationException($"Action {Name} does not implement IAction");
        }
        
        actionInterface.Initialize(platform);
        
        return actionInterface;
    }
}
