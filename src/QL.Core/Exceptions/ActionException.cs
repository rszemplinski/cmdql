namespace QL.Core.Exceptions;

public class ActionException(string message, int exitCode) : Exception(message.ReplaceLineEndings(""))
{
    public int ExitCode { get; } = exitCode;
}