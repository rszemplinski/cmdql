using QL.Core;

namespace QLShell.Sessions;

public class CommandOutput : ICommandOutput
{
    /**
     * The output of the command that was executed.
     */
    public string Result { get; set; }
    
    /**
     * The error message of the command that was executed.
     */
    public string Error { get; set; }
    
    /**
     * The exit code of the command that was executed.
     */
    public int ExitCode { get; set; }
}