using System.Runtime.InteropServices;
using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;
using Serilog;

namespace QL.Actions.Standard.GetLogs;

public record GetLogsArguments
{
    /**
     * The start date to get logs from (format: yyyy-MM-dd ie. 2021-01-01)
     */
    public DateTime StartDate { get; set; } = default;

    /**
     * The end date to get logs from (format: yyyy-MM-dd ie. 2021-01-01)
     */
    public DateTime EndDate { get; set; } = default;

    /**
     * The maximum number of logs to get (ie. 100)
     */
    public int Limit { get; set; } = -1;

    /**
     * Most recent logs to get (ie. 10)
     */
    public int Top { get; set; } = -1;
}

public class LogEntry
{
    /**
     * The log message. (Includes the service name)
     */
    public string Message { get; set; }

    /**
     * The service that generated the log (ie. systemd, sshd, etc.)
     */
    public string Service { get; set; }

    /**
     * The machine name that generated the log
     */
    public string MachineName { get; set; }

    /**
     * The timestamp of the log (ie. Jan 01 00:00:00)
     */
    public string Timestamp { get; set; }
}

[Action(
    Description = "Retrieve logs from the system journal"
)]
public class GetLogs : ActionBase<GetLogsArguments, List<LogEntry>>
{
    protected override string BuildCommand(GetLogsArguments arguments)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return BuildLinuxCommand(arguments);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return BuildMacCommand(arguments);
        }

        throw new ArgumentOutOfRangeException();
    }

    /**
     * Example line from `journalctl`:
     * Dec 27 10:17:24 pop-os /usr/libexec/gdm-x-session[6513]: (II) AMDGPU(0): Modeline "800x600"x0.0   40.00  800 840 968 1056  600 601 605 628 +hsync +vsync (37.9 kHz e)
     * Dec 30 23:48:30 pop-os systemd[1]: Finished Message of the Day.
     * Dec 30 23:48:30 pop-os systemd[1]: motd-news.service: Succeeded.
     */
    protected override List<LogEntry> ParseCommandResults(ICommandOutput commandResults)
    {
        var logEntries = new List<LogEntry>();
        var lines = commandResults.Result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var logEntry = new LogEntry();
            var parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                continue;
            }

            logEntry.Timestamp = $"{parts[0]} {parts[1]} {parts[2]}";
            logEntry.MachineName = parts[3];
            logEntry.Service = parts[4].TrimEnd(':');

            var message = string.Join(" ", parts[4..]);
            logEntry.Message = message;

            logEntries.Add(logEntry);
        }

        return logEntries;
    }

    private static string BuildLinuxCommand(GetLogsArguments arguments)
    {
        var command = "journalctl";
        if (arguments.StartDate != default)
        {
            command += $" --since \"{arguments.StartDate:yyyy-MM-dd}\"";
        }

        if (arguments.EndDate != default)
        {
            command += $" --until \"{arguments.EndDate:yyyy-MM-dd}\"";
        }

        if (arguments.Limit > 0)
        {
            command += $" --lines {arguments.Limit}";
        }

        if (arguments.Top > 0)
        {
            command += $" --reverse --lines {arguments.Top}";
        }

        return command;
    }

    private static string BuildMacCommand(GetLogsArguments arguments)
    {
        var command = "log show";
        if (arguments.StartDate != default)
        {
            command += $" --start \"{arguments.StartDate:yyyy-MM-dd}\"";
        }

        if (arguments.EndDate != default)
        {
            command += $" --end \"{arguments.EndDate:yyyy-MM-dd}\"";
        }

        if (arguments.Limit > 0)
        {
            command += $" --last {arguments.Limit}";
        }

        if (arguments.Top > 0)
        {
            command += $" --reverse --last {arguments.Top}";
        }

        return command;
    }
}