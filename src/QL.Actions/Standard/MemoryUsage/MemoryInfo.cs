namespace QL.Actions.Standard.MemoryUsage;

public class MemoryInfo
{
    public ulong Total { get; set; }
    public ulong Used { get; set; }
    public ulong Free { get; set; }
    public ulong SwapTotal { get; set; }
    public ulong SwapUsed { get; set; }
    public ulong SwapFree { get; set; }
}