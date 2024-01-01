using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.MemoryUsage;

public class MemoryUsageArguments;

[Action]
public class MemoryUsage : ActionBase<MemoryUsageArguments, MemoryInfo>
{
    protected override string BuildCommand(MemoryUsageArguments arguments)
    {
        return "free -m";
    }
}