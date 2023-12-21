namespace QL.Actions.Core;

public interface IAction
{
    public string BuildCommand(IDictionary<string, object> arguments);
    
    public object ParseCommandResults(string commandResult);
}