namespace QL.Actions.Standard.Memory;

public class Memory
{
    public ulong Total { get; set; }
    public ulong Used { get; set; }
    public ulong Free { get; set; }
    public ulong Shared { get; set; }
    public ulong BuffCache { get; set; }
    public ulong Available { get; set; }
}

public class Swap
{
    public ulong Total { get; set; }
    public ulong Used { get; set; }
    public ulong Free { get; set; }
}

public class MemoryInfoResult
{
    public Memory Memory { get; set; }
    public Swap Swap { get; set; }
}