using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.CurrentTime;

public enum AMPM
{
    AM,
    PM
}

/// <summary>
/// Result of the <see cref="CurrentTime"/> action
/// </summary>
public class CurrentTimeResult
{
    /// <summary>
    /// Day of the week as a string (e.g. "Mon", "Tue", "Wed", etc.)
    /// </summary>
    public string DayOfWeek { get; set; }

    /// <summary>
    /// Month as a string (e.g. "Jan", "Feb", "Mar", etc.)
    /// </summary>
    public string Month { get; set; }

    /// <summary>
    /// Day of the month as an integer
    /// </summary>
    public int Day { get; set; }

    /// <summary>
    /// Time as a string (e.g. "12:00:00")
    /// </summary>
    public string Time { get; set; }

    /// <summary>
    /// Time zone as a string (e.g. "EST", "EDT", "CST", "CDT", etc.)
    /// </summary>
    public string TimeZone { get; set; }

    /// <summary>
    /// Year as an integer
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// AM/PM value
    /// </summary>
    public AMPM AMPM { get; set; }
}

/// <summary>
/// Runs the <c>date</c> command and returns the current date and time
/// </summary>
[Action]
[Cmd("date")]
public class CurrentTime : ActionBase<CurrentTimeResult>
{
    protected override CurrentTimeResult ParseCommandResults(ICommandOutput commandResults)
    {
        var result = new CurrentTimeResult();

        var lines = commandResults.Result.Split(Environment.NewLine);
        var line = lines[0];

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        result.DayOfWeek = parts[0];
        result.Month = parts[1];
        result.Day = int.Parse(parts[2]);
        result.Time = parts[3];

        // Check if parts[4] is a AM/PM value
        if (parts[4] == "AM" || parts[4] == "PM")
        {
            result.TimeZone = parts[5];
            result.Year = int.Parse(parts[6]);
            result.AMPM = parts[4] == "AM" ? AMPM.AM : AMPM.PM;
        }
        else
        {
            result.TimeZone = parts[4];
            result.Year = int.Parse(parts[5]);

            var timeParts = result.Time.Split(':');
            var hour = int.Parse(timeParts[0]);
            result.AMPM = hour >= 12 ? AMPM.PM : AMPM.AM;
        }

        return result;
    }
}