namespace QL.Actions.Standard.CPUInfo;

public class FrequencyRangeInfo
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
    public decimal BogoMIPS { get; set; }
    public bool FrequencyBoost { get; set; }
}

public class CacheInfo
{
    public string Type { get; set; }
    public uint Size { get; set; }
    public uint Instances { get; set; }
}

public class NUMAInfo
{
    public uint Nodes { get; set; }
}

public class CPU
{
    public string Architecture { get; set; }
    public string AddressSizes { get; set; }
    public string CPUOpModes { get; set; }
    public string ByteOrder { get; set; }
    public uint Cpus { get; set; }
    public uint ThreadsPerCore { get; set; }
    public uint CoresPerSocket { get; set; }
    public uint Sockets { get; set; }
    public uint Stepping { get; set; }
    public FrequencyRangeInfo FrequencyRange { get; set; }
    
    public bool SMT => ThreadsPerCore > 1;
}

public class VendorInfo
{
    public string VendorID { get; set; }
    public string CPUFamily { get; set; }
    public string Model { get; set; }
    public string ModelName { get; set; }
}

public class VulnerabilityInfo
{
    public string Name { get; set; }
    public string Status { get; set; }
}

public class CPUInfoResults
{
    public CPU CPU { get; set; }
    public string Virtualization { get; set; }
    public List<string> Flags { get; set; }
    public List<CacheInfo> Caches { get; set; }
    public NUMAInfo NUMA { get; set; }
    public VendorInfo Vendor { get; set; }
    public List<VulnerabilityInfo> Vulnerabilities { get; set; }
}