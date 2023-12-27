using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Processes;

public class ProcessArguments
{
    /**
     * The top of processes to return.
     */
    public int? Top { get; set; }
    
    /**
     * The limit of processes to return.
     */
    public int? Limit { get; set; }
}

[Action]
public class Processes : ActionBase<ProcessArguments, List<Process>>
{
    protected override Task<string> _BuildCommandAsync(ProcessArguments arguments)
    {
        var command = "ps -eo pid,user,command,%cpu,%mem";
        if (arguments.Top.HasValue)
        {
            command += $" | head -n {arguments.Top}";
        }
        else if (arguments.Limit.HasValue)
        {
            command += $" | tail -n {arguments.Limit}";
        }
        return Task.FromResult(command);
    }
}
