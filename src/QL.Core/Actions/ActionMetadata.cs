namespace QL.Core.Actions;

public record ActionMetadata(string Name, string? Description, Type Type)
{
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
