namespace QL.Actions.Standard.Processes;

public class Process
{
    public string Pid { get; }
    public string User { get; }
    public string Command { get; }
    public float CpuUsage { get; }
    public float MemoryUsage { get; }
}