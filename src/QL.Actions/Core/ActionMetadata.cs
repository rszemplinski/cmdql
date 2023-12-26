using QL.Actions.Core.Actions;

namespace QL.Actions.Core;

public record ActionMetadata(string Name, string? Description, Type Type)
{
    public IAction CreateAction()
    {
        var action = Activator.CreateInstance(Type);
        if (action is not IAction actionInterface)
        {
            throw new InvalidOperationException($"Action {Name} does not implement IAction");
        }
        
        return actionInterface;
    }
}
