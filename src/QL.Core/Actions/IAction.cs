namespace QL.Core.Actions;

public interface IAction
{
    public Task<string> BuildCommandAsync(Dictionary<string, object> arguments);
    
    public Task<string> PostExecutionAsync(string commandResult, ISshClient sshClient);
    
    public Task<object> ParseCommandResultsAsync(string commandResult, string[] fields);
}