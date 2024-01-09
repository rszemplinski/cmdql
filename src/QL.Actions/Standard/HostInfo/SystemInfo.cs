using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.HostInfo;

public class SystemInfoResults
{
    public string KernelName { get; set; }
    public string HostName { get; set; }
    public string KernelRelease { get; set; }
    public string VersionSignature { get; set; }
    public string KernelBuildOptions { get; set; }
    public string Architecture { get; set; }
    public string OS { get; set; }
}

[Action]
[Cmd("uname -a")]
public partial class SystemInfo : ActionBase<SystemInfoResults>
{
    protected override SystemInfoResults? ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => ParseLinux(commandResults),
            Platform.OSX => ParseOSX(commandResults),
            _ => throw new PlatformNotSupportedException()
        };
    }
    
    private static SystemInfoResults? ParseLinux(ICommandOutput commandResults)
    {
        var regex = LinuxUnameRegex();
        var match = regex.Match(commandResults.Result);
        if (!match.Success)
        {
            return null;
        }
        
        return new SystemInfoResults
        {
            KernelName = match.Groups["KernelName"].Value,
            HostName = match.Groups["NodeName"].Value,
            KernelRelease = match.Groups["KernelRelease"].Value,
            VersionSignature = match.Groups["VersionSignature"].Value,
            KernelBuildOptions = match.Groups["BuildOptions"].Value,
            Architecture = match.Groups["Architecture1"].Value,
            OS = match.Groups["OperatingSystem"].Value
        };
    }
    
    private static SystemInfoResults? ParseOSX(ICommandOutput commandResults)
    {
        var regex = MacUnameRegex();
        var match = regex.Match(commandResults.Result);
        if (!match.Success)
        {
            return null;
        }
        
        return new SystemInfoResults
        {
            KernelName = match.Groups["KernelName"].Value,
            HostName = match.Groups["NodeName"].Value,
            KernelRelease = match.Groups["KernelVersion"].Value,
            VersionSignature = match.Groups["KernelFullVersion"].Value,
            KernelBuildOptions = match.Groups["BuildTime"].Value,
            Architecture = match.Groups["Architecture"].Value,
            OS = "Darwin"
        };
    }

    [GeneratedRegex(@"^(?<KernelName>\S+)\s+(?<NodeName>\S+)\s+(?<KernelVersion>\S+)\s+(?<KernelFullVersion>[\w\s]+):\s+(?<BuildTime>.+?);\s+root:(?<RootInfo>[\w~.]+)\s+(?<Architecture>\S+)$")]
    private static partial Regex MacUnameRegex();
    
    [GeneratedRegex(@"^(?<KernelName>\S+)\s+(?<NodeName>\S+)\s+(?<KernelRelease>\S+)\s+(?<VersionSignature>\S+)\s+(?<BuildOptions>[\w\s]+)\s+(?<Architecture>\S+)\s+\S+\s+\S+\s+(?<OperatingSystem>.+)$")]
    private static partial Regex LinuxUnameRegex();
}
