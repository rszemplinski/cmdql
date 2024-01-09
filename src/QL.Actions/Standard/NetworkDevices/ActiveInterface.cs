using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.NetworkDevices;

[Action]
public class ActiveInterface : ActionBase<string>
{
    protected override string BuildCommand(object arguments)
    {
        return Platform switch
        {
            Platform.Linux => "ip route show default | awk '/default/ {print $5}'",
            Platform.OSX => "route -n get default | awk '/interface:/{print $2}'",
            _ => throw new PlatformNotSupportedException()
        };
    }

    protected override string? ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => commandResults.Result.Trim(),
            Platform.OSX => commandResults.Result.Trim(),
            _ => throw new PlatformNotSupportedException()
        };
    }
}