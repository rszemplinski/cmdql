using System.Globalization;
using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

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
        return Platform switch
        {
            Platform.Linux => BuildLinuxCommand(arguments),
            Platform.OSX => BuildMacCommand(),
            _ => throw new PlatformNotSupportedException()
        };
    }

    /**
     * Example line from `journalctl`:
     * Dec 27 10:17:24 pop-os /usr/libexec/gdm-x-session[6513]: (II) AMDGPU(0): Modeline "800x600"x0.0   40.00  800 840 968 1056  600 601 605 628 +hsync +vsync (37.9 kHz e)
     * Dec 30 23:48:30 pop-os systemd[1]: Finished Message of the Day.
     * Dec 30 23:48:30 pop-os systemd[1]: motd-news.service: Succeeded.
     */
    protected override List<LogEntry> ParseCommandResults(ICommandOutput commandResults)
    {
        return Platform switch
        {
            Platform.Linux => ParseCommandResultForLinux(commandResults),
            Platform.OSX => ParseCommandResultForMac(commandResults, GetArguments()),
            _ => throw new PlatformNotSupportedException()
        };
    }

    private static List<LogEntry> ParseCommandResultForLinux(ICommandOutput commandResults)
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

        Array.Clear(lines, 0, lines.Length);

        return logEntries;
    }

    private static List<LogEntry> ParseCommandResultForMac(ICommandOutput commandResults, GetLogsArguments arguments)
    {
        var logEntries = new List<LogEntry>();
        var lines = commandResults.Result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var maxResults = arguments.Top > 0 ? arguments.Top : int.MaxValue;
        var startDateTime = arguments.StartDate != default ? arguments.StartDate : DateTime.MinValue;
        var endDateTime = arguments.EndDate != default ? arguments.EndDate : DateTime.MaxValue;

        for (var i = 0; i < lines.Length; i++)
        {
            var parts = lines[i].Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
            {
                continue;
            }

            var timestampString = $"{parts[0]} {parts[1]} {parts[2]}";
            if (!DateTime.TryParseExact(timestampString, ["MMM dd HH:mm:ss", "MMM d HH:mm:ss"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
            {
                continue;
            }

            if (timestamp > DateTime.UtcNow)
            {
                timestamp = timestamp.AddYears(-1);
            }

            if (timestamp < startDateTime || timestamp > endDateTime)
            {
                continue;
            }

            var message = string.Join(" ", parts[4..]);
            logEntries.Add(new LogEntry
            {
                Timestamp = timestampString,
                MachineName = parts[3],
                Service = parts[4].TrimEnd(':'),
                Message = message
            });

            if (logEntries.Count >= maxResults)
                break;
        }

        Array.Clear(lines, 0, lines.Length);

        return logEntries;
    }

    private static string BuildLinuxCommand(GetLogsArguments arguments)
    {
        var command = "journalctl --reverse";
        if (arguments.StartDate != default)
        {
            command += $" --since \"{arguments.StartDate:yyyy-MM-dd}\"";
        }

        if (arguments.EndDate != default)
        {
            command += $" --until \"{arguments.EndDate:yyyy-MM-dd}\"";
        }

        if (arguments.Top > 0)
        {
            command += $" --lines {arguments.Top}";
        }

        return command;
    }

    private static string BuildMacCommand()
    {
        const string logDir = "/var/log";

        const string command = $"for file in {logDir}/system.log*; do " +
                               $"if [[ $file =~ \\.gz$ ]]; then " +
                               $"gunzip -c \"$file\"; " +
                               $@"elif [[ $file =~ system\.log\.[0-9]+$ ]]; then " +
                               $"cat \"$file\"; " +
                               $"fi; " +
                               "done";

        return command;
    }
}