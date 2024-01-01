using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.RunCustom;

public class RunCustomArguments
{
    public string Cmd { get; set; }
}

public class RunCustomResult
{
    public string Output { get; set; }
    public int ExitCode { get; set; }
}

[Action]
[Cmd("{cmd}")]
public class RunCustom : ActionBase<RunCustomArguments, RunCustomResult>
{
    protected override RunCustomResult ParseCommandResults(ICommandOutput commandResults)
    {
        return new RunCustomResult
        {
            Output = commandResults.Result,
            ExitCode = commandResults.ExitCode
        };
    }
}