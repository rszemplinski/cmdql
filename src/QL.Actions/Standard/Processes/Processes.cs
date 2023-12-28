using System.Text.RegularExpressions;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.Processes;

public class ProcessArguments
{
    /**
     * The limit of processes to return.
     */
    public int Limit { get; set; } = -1;
}

[Action]
public partial class Processes : ActionBase<ProcessArguments, List<Process>>
{
    protected override Task<string> _BuildCommandAsync(ProcessArguments arguments)
    {
        var command = "ps -reo pid,user,args,%cpu,%mem,etime,flags";
        if (arguments.Limit > 0)
        {
            command += $" | head -n {arguments.Limit + 1}";
        }

        return Task.FromResult(command);
    }

    protected override Task<List<Process>> _ParseCommandResultsAsync(string commandResults)
    {
        var processes = new List<Process>();
        var regex = ProcessRegex();
        var matchCollection = regex.Matches(commandResults);

        foreach (Match match in matchCollection)
        {
            var process = new Process
            {
                PID = int.Parse(match.Groups[1].Value),
                User = match.Groups[2].Value,
                Command = match.Groups[3].Value,
                CpuUsage = float.Parse(match.Groups[4].Value),
                MemoryUsage = float.Parse(match.Groups[5].Value),
                ElapsedTime = ParseTimeSpan(match.Groups[6].Value),
                Flags = ParseFlags(match.Groups[7].Value),
            };

            processes.Add(process);
        }

        return Task.FromResult(processes);
    }

    private static List<ProcessFlag> ParseFlags(string hexString)
    {
        var flagValue = Convert.ToInt32(hexString, 16);
        return Enum.GetValues(typeof(ProcessFlag))
            .Cast<ProcessFlag>()
            .Where(flag => (flagValue & (int)flag) != 0)
            .ToList();
    }

    private static TimeSpan ParseTimeSpan(string timeString)
    {
        var parts = timeString.Split(':');
        int days = 0, hours = 0, minutes = 0, seconds = 0;

        switch (parts.Length)
        {
            // mm:ss
            case 2:
                minutes = int.Parse(parts[0]);
                seconds = int.Parse(parts[1]);
                break;
            // hh:mm:ss or dd-hh:mm
            // dd-hh:mm
            case 3 when parts[0].Contains('-'):
            {
                var dayHour = parts[0].Split('-');
                days = int.Parse(dayHour[0]);
                hours = int.Parse(dayHour[1]);
                minutes = int.Parse(parts[1]);
                seconds = int.Parse(parts[2]);
                break;
            }
            // hh:mm:ss
            case 3:
                hours = int.Parse(parts[0]);
                minutes = int.Parse(parts[1]);
                seconds = int.Parse(parts[2]);
                break;
            // dd-hh:mm:ss
            case 4:
                days = int.Parse(parts[0].Split('-')[0]);
                hours = int.Parse(parts[0].Split('-')[1]);
                minutes = int.Parse(parts[1]);
                seconds = int.Parse(parts[2]);
                break;
            default:
                throw new FormatException("Invalid time format.");
        }

        return new TimeSpan(days, hours, minutes, seconds);
    }

    [GeneratedRegex(
        @"\s*(\d+)\s+(\S+)\s+(.*?)\s+(\d+\.\d+)\s+(\d+\.\d+)\s+((?:\d+-)?(?:\d{2}:)?\d{2}:\d{2})\s+(\S+)",
        RegexOptions.Multiline)]
    private static partial Regex ProcessRegex();
}