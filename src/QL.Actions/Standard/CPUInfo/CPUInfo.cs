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

    private static string BuildMacCommand()
    {
        var builder = new CommandBuilder();
        builder.AddConcatenatedCommand("sysctl -a | grep machdep.cpu");
        builder.AddConcatenatedCommand("sysctl -a | grep cpu | grep hw");
        builder.AddConcatenatedCommand("sysctl -a | grep cache");
        builder.AddConcatenatedCommand("arch");
        return builder.Build();
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
        var reFrequency =
            new Regex(
                @"CPU max MHz:\s+(?<max>\d+(\.\d+)?)\s+CPU min MHz:\s+(?<min>\d+(\.\d+)?)\s+BogoMIPS:\s+(?<bogo>\d+(\.\d+)?)");
        var reFrequencyBoost = new Regex(@"Frequency boost:\s+(?<val>enabled|disabled)");
        var reCache =
            new Regex(
                @"(?<type>L\d[di]?) cache:\s+(?<size>\d+)\s+(?<unit>KiB|MiB)\s+\((?<instances>\d+) instance[s]?\)");
        var reVendor =
            new Regex(
                @"Vendor ID:\s+(?<vendorID>.+)\n.*Model name:\s+(?<modelName>.+)\n.*CPU family:\s+(?<cpuFamily>.+)\n.*Model:\s+(?<model>.+)");
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
        if (threadsPerCoreMatch.Success &&
            uint.TryParse(threadsPerCoreMatch.Groups["val"].Value, out var threadsPerCore))
        {
            cpuInfoResults.CPU.ThreadsPerCore = threadsPerCore;
        }

        // Parse Cores per Socket
        var coresPerSocketMatch = reCoresPerSocket.Match(input);
        if (coresPerSocketMatch.Success &&
            uint.TryParse(coresPerSocketMatch.Groups["val"].Value, out var coresPerSocket))
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
            cpuInfoResults.Flags.AddRange(flagsMatch.Groups["flags"].Value
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
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

    private static CPUInfoResults? ParseOSX(ICommandOutput commandResults)
    {
        var lines = commandResults.Result.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var data = new Dictionary<string, string>();
        string? architecture = null;

        foreach (var line in lines)
        {
            var parts = line.Split(new[] { ':' }, 2);
            switch (parts.Length)
            {
                // Check for key-value pair
                case 2:
                    data[parts[0].Trim()] = parts[1].Trim();
                    break;
                // Handle standalone values like architecture
                case 1:
                    architecture = parts[0].Trim();
                    break;
            }
        }
        
        var cpuInfoResults = new CPUInfoResults
        {
            CPU = new CPU
            {
                Architecture = architecture,
                Cpus = GetUIntValue(data, "hw.ncpu"),
                CoresPerSocket = GetUIntValue(data, "machdep.cpu.cores_per_package"),
                ThreadsPerCore = CalculateThreadsPerCore(data),
                
            },
            Vendor = new VendorInfo
            {
                ModelName = data.GetValueOrDefault("machdep.cpu.brand_string")
            },
            Caches = ParseCacheInfo(data),
            Vulnerabilities = []
        };

        return cpuInfoResults;
    }
    
    private static List<CacheInfo> ParseCacheInfo(Dictionary<string, string> data)
    {
        var cacheInfoList = new List<CacheInfo>();

        // Parse general cache sizes
        AddCacheInfo(cacheInfoList, "L1 Instruction", GetUIntValue(data, "hw.l1icachesize"));
        AddCacheInfo(cacheInfoList, "L1 Data", GetUIntValue(data, "hw.l1dcachesize"));
        AddCacheInfo(cacheInfoList, "L2", GetUIntValue(data, "hw.l2cachesize"));

        // Parse cache sizes for performance levels
        var perfLevels = new[] { "perflevel0", "perflevel1" };
        foreach (var level in perfLevels)
        {
            AddCacheInfo(cacheInfoList, $"{level} L1 Instruction", GetUIntValue(data, $"hw.{level}.l1icachesize"));
            AddCacheInfo(cacheInfoList, $"{level} L1 Data", GetUIntValue(data, $"hw.{level}.l1dcachesize"));
            AddCacheInfo(cacheInfoList, $"{level} L2", GetUIntValue(data, $"hw.{level}.l2cachesize"));
        }

        return cacheInfoList;
    }
    
    private static void AddCacheInfo(List<CacheInfo> cacheInfoList, string type, uint size)
    {
        if (size > 0)
        {
            cacheInfoList.Add(new CacheInfo { Type = type, Size = size });
        }
    }
    
    private static uint GetUIntValue(IReadOnlyDictionary<string, string?> data, string key)
    {
        if (!data.TryGetValue(key, out var value)) return 0;
        return uint.TryParse(value, out var result) ? result : 0;
    }

    private static uint CalculateThreadsPerCore(IReadOnlyDictionary<string, string?> data)
    {
        var threadCount = GetUIntValue(data, "machdep.cpu.thread_count");
        var coreCount = GetUIntValue(data, "machdep.cpu.cores_per_package");

        // Avoid division by zero; if core count is zero, return 0 for threads per core
        return coreCount > 0 ? threadCount / coreCount : 0;
    }

    private static uint ConvertSizeToBytes(uint size, string unit)
    {
        return unit == "MiB" ? size * 1024 * 1024 : size * 1024;
    }
}