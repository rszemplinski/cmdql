using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Memory;

[Action]
public class MemoryInfo : ActionBase<MemoryInfoResult>
{
    protected override string BuildCommand(object arguments)
    {
        return Platform switch
        {
            Platform.Linux => "free -b",
            Platform.OSX => BuildMacCommand(),
            _ => throw new PlatformNotSupportedException()
        };
    }

    protected override MemoryInfoResult? ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => ParseLinux(commandResults),
            Platform.OSX => null,
            _ => throw new PlatformNotSupportedException()
        };
    }
    
    private static MemoryInfoResult? ParseLinux(ICommandOutput commandResults)
    {
        var input = commandResults.Result;
        var lines = input.Split("\n");
        var memoryLine = lines[1];
        var swapLine = lines[2];

        var memoryValues = memoryLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var swapValues = swapLine.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        
        var memory = new Memory
        {
            Total = ulong.Parse(memoryValues[1]),
            Used = ulong.Parse(memoryValues[2]),
            Free = ulong.Parse(memoryValues[3]),
            Shared = ulong.Parse(memoryValues[4]),
            BuffCache = ulong.Parse(memoryValues[5]),
            Available = ulong.Parse(memoryValues[6])
        };
        
        var swap = new Swap
        {
            Total = ulong.Parse(swapValues[1]),
            Used = ulong.Parse(swapValues[2]),
            Free = ulong.Parse(swapValues[3])
        };
        
        return new MemoryInfoResult
        {
            Memory = memory,
            Swap = swap
        };
    }

    // TODO: Implement for macOS
    private static string BuildMacCommand()
    {
        return "sysctl -a | grep hw.memsize";
    }
}