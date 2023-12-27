namespace QL.Actions.Standard.Processes;

[Flags]
public enum ProcessFlag
{
    P_ADVLOCK = 0x00000001,
    P_CONTROLT = 0x00000002,
    P_LP64 = 0x00000004,
    P_NOCLDSTOP = 0x00000008,
    P_PPWAIT = 0x00000010,
    P_PROFIL = 0x00000020,
    P_SELECT = 0x00000040,
    P_CONTINUED = 0x00000080,
    P_SUGID = 0x00000100,
    P_SYSTEM = 0x00000200,
    P_TIMEOUT = 0x00000400,
    P_TRACED = 0x00000800,
    P_WAITED = 0x00001000,
    P_WEXIT = 0x00002000,
    P_EXEC = 0x00004000,
    P_OWEUPC = 0x00008000,
    P_WAITING = 0x00010000,
    P_KDEBUG = 0x00020000,
}

/**
 * This contains information about a process.
 */
public class Process
{
   /**
    * The process ID.
    */
    public int PID { get; set; }
    
    /**
     * The name of the user who started the process.
     */
    public string User { get; set; }
    
    /**
     * The name of the process.
     */
    public string Command { get; set; }
    
    /**
     * The amount of CPU time used by the process.
     */
    public float CpuUsage { get; set; }
    
    /**
     * The amount of memory currently used by the process.
     */
    public float MemoryUsage { get; set; }
    
    /**
     * The elapsed time since the process started.
     */
    public TimeSpan ElapsedTime { get; set; }
    
    /**
     * Process flags.
     */
    public List<ProcessFlag> Flags { get; set; } = [];
}