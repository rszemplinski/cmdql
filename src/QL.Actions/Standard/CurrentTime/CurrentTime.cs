using QL.Core;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.CurrentTime;

public enum AMPM
{
    AM,
    PM
}

public class CurrentTimeResult
{
    /**
     * Day of the week as a string (e.g. "Mon", "Tue", "Wed", etc.)
     */
    public string DayOfWeek { get; set; }

    /**
     * Month as a string (e.g. "Jan", "Feb", "Mar", etc.)
     */
    public string Month { get; set; }

    /**
     * Day of the month as an integer
     */
    public int Day { get; set; }

    /**
     * Time as a string (e.g. "12:00:00")
     */
    public string Time { get; set; }

    /**
     * Time zone as a string (e.g. "PST", "MST", "EST", etc.)
     */
    public string TimeZone { get; set; }

    /**
     * Year as an integer
     */
    public int Year { get; set; }

    /**
     * AM or PM
     */
    public AMPM AMPM { get; set; }
}

[Action]
[Cmd("date")]
public class CurrentTime : ActionBase<CurrentTimeResult>
{
    /**
     * Example:
     *  Wed Jan  3 21:20:55 MST 2024
     *  Wed Jan  3 09:21:43 PM MST 2024
     */
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