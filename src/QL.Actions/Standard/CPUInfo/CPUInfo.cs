using System.Text.RegularExpressions;
using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.CPUInfo;

[Action]
public class CPUInfo : ActionBase<CPUInfoResults>
{
    protected override string BuildCommand(object arguments)
    {
        return Platform switch
        {
            Platform.Linux => "lscpu",
            Platform.OSX => BuildMacCommand(),
            _ => throw new PlatformNotSupportedException()
        };
    }
    
    // TODO: Implement for macOS
    private static string BuildMacCommand()
    {
        return "sysctl -a | grep machdep.cpu";
    }

    protected override CPUInfoResults? ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => ParseLinux(commandResults),
            Platform.OSX => ParseOSX(commandResults),
            _ => throw new PlatformNotSupportedException()
        };
    }
    
    private static CPUInfoResults? ParseLinux(ICommandOutput commandResults)
    {
        var input = commandResults.Result;
        var cpuInfoResults = new CPUInfoResults
        {
            CPU = new CPU(),
            Flags = [],
            Caches = [],
            NUMA = new NUMAInfo(),
            Vendor = new VendorInfo(),
            Virtualization = string.Empty,
            Vulnerabilities = [],
        };

        // Regular expressions for different parts
        var reArchitecture = new Regex(@"Architecture:\s+(?<val>\S+)");
        var reAddressSizes = new Regex(@"Address sizes:\s+(?<val>.+)");
        var reCPUOpModes = new Regex(@"CPU op-mode\(s\):\s+(?<val>.+)");
        var reByteOrder = new Regex(@"Byte Order:\s+(?<val>.+)");
        var reCPUs = new Regex(@"CPU\(s\):\s+(?<val>\d+)");
        var reThreadsPerCore = new Regex(@"Thread\(s\) per core:\s+(?<val>\d+)");
        var reCoresPerSocket = new Regex(@"Core\(s\) per socket:\s+(?<val>\d+)");
        var reSockets = new Regex(@"Socket\(s\):\s+(?<val>\d+)");
        var reStepping = new Regex(@"Stepping:\s+(?<val>\d+)");
        var reFrequency = new Regex(@"CPU max MHz:\s+(?<max>\d+(\.\d+)?)\s+CPU min MHz:\s+(?<min>\d+(\.\d+)?)\s+BogoMIPS:\s+(?<bogo>\d+(\.\d+)?)");
        var reFrequencyBoost = new Regex(@"Frequency boost:\s+(?<val>enabled|disabled)");
        var reCache = new Regex(@"(?<type>L\d[di]?) cache:\s+(?<size>\d+)\s+(?<unit>KiB|MiB)\s+\((?<instances>\d+) instance[s]?\)");
        var reVendor = new Regex(@"Vendor ID:\s+(?<vendorID>.+)\n.*Model name:\s+(?<modelName>.+)\n.*CPU family:\s+(?<cpuFamily>.+)\n.*Model:\s+(?<model>.+)");
        var reFlags = new Regex(@"Flags:\s+(?<flags>[^\n]+)(?=\n\S+:|$)");
        var reNUMA = new Regex(@"NUMA node\(s\):\s+(?<nodes>\d+)");
        var reVirtualization = new Regex(@"Virtualization:\s+(?<virtualization>\S+)");
        var reVulnerabilities = new Regex(@"Vulnerability (?<vulnerability>[\w\s]+):\s+(?<status>.+)");

        // Parse architecture, sizes, and op-modes
        cpuInfoResults.CPU.Architecture = reArchitecture.Match(input).Groups["val"].Value;
        cpuInfoResults.CPU.AddressSizes = reAddressSizes.Match(input).Groups["val"].Value;
        cpuInfoResults.CPU.CPUOpModes = reCPUOpModes.Match(input).Groups["val"].Value;
        cpuInfoResults.CPU.ByteOrder = reByteOrder.Match(input).Groups["val"].Value;

        // Parse CPUs
        if (uint.TryParse(reCPUs.Match(input).Groups["val"].Value, out var cpus))
        {
            cpuInfoResults.CPU.Cpus = cpus;
        }
        
        var threadsPerCoreMatch = reThreadsPerCore.Match(input);
        if (threadsPerCoreMatch.Success && uint.TryParse(threadsPerCoreMatch.Groups["val"].Value, out var threadsPerCore))
        {
            cpuInfoResults.CPU.ThreadsPerCore = threadsPerCore;
        }

        // Parse Cores per Socket
        var coresPerSocketMatch = reCoresPerSocket.Match(input);
        if (coresPerSocketMatch.Success && uint.TryParse(coresPerSocketMatch.Groups["val"].Value, out var coresPerSocket))
        {
            cpuInfoResults.CPU.CoresPerSocket = coresPerSocket;
        }

        // Parse Sockets
        var socketsMatch = reSockets.Match(input);
        if (socketsMatch.Success && uint.TryParse(socketsMatch.Groups["val"].Value, out var sockets))
        {
            cpuInfoResults.CPU.Sockets = sockets;
        }

        // Parse Stepping
        var steppingMatch = reStepping.Match(input);
        if (steppingMatch.Success && uint.TryParse(steppingMatch.Groups["val"].Value, out var stepping))
        {
            cpuInfoResults.CPU.Stepping = stepping;
        }

        // Parse frequency information
        var freqMatch = reFrequency.Match(input);
        cpuInfoResults.CPU.FrequencyRange = new FrequencyRangeInfo
        {
            Min = decimal.Parse(freqMatch.Groups["min"].Value),
            Max = decimal.Parse(freqMatch.Groups["max"].Value),
            BogoMIPS = decimal.Parse(freqMatch.Groups["bogo"].Value),
            FrequencyBoost = reFrequencyBoost.Match(input).Groups["val"].Value == "enabled"
        };

        // Parse cache information
        var cacheMatches = reCache.Matches(input);
        foreach (Match match in cacheMatches)
        {
            var sizeInKiB = ConvertSizeToBytes(uint.Parse(match.Groups["size"].Value), match.Groups["unit"].Value);
            cpuInfoResults.Caches.Add(new CacheInfo
            {
                Type = match.Groups["type"].Value,
                Size = sizeInKiB,
                Instances = uint.Parse(match.Groups["instances"].Value)
            });
        }
        
        var vendorMatch = reVendor.Match(input);
        if (vendorMatch.Success)
        {
            cpuInfoResults.Vendor = new VendorInfo
            {
                VendorID = vendorMatch.Groups["vendorID"].Value.Trim(),
                ModelName = vendorMatch.Groups["modelName"].Value.Trim(),
                CPUFamily = vendorMatch.Groups["cpuFamily"].Value.Trim(),
                Model = vendorMatch.Groups["model"].Value.Trim()
            };
        }

        // Parse flags
        var flagsMatch = reFlags.Match(input);
        if (flagsMatch.Success)
        {
            cpuInfoResults.Flags.AddRange(flagsMatch.Groups["flags"].Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        // Parse NUMA information
        var numaMatch = reNUMA.Match(input);
        if (numaMatch.Success && uint.TryParse(numaMatch.Groups["nodes"].Value, out var nodes))
        {
            cpuInfoResults.NUMA.Nodes = nodes;
        }

        // Parse Virtualization information
        var virtualizationMatch = reVirtualization.Match(input);
        if (virtualizationMatch.Success)
        {
            cpuInfoResults.Virtualization = virtualizationMatch.Groups["virtualization"].Value;
        }
        
        var vulnerabilityMatches = reVulnerabilities.Matches(input);

        foreach (Match match in vulnerabilityMatches)
        {
            cpuInfoResults.Vulnerabilities.Add(new VulnerabilityInfo
            {
                Name = match.Groups["vulnerability"].Value.Trim(),
                Status = match.Groups["status"].Value.Trim()
            });
        }

        return cpuInfoResults;
    }

    // TODO: Implement for macOS
    private static CPUInfoResults? ParseOSX(ICommandOutput commandResults)
    {
        return null;
    }
    
    private static uint ConvertSizeToBytes(uint size, string unit)
    {
        return unit == "MiB" ? size * 1024 * 1024 : size * 1024;
    }
}