namespace QL.Actions.Core.Actions;

public interface IAction
{
    public string BuildCommand(Dictionary<string, object> arguments);
    
    public object ParseCommandResults(string commandResult);
}