using System.Globalization;
using QL.Core.Actions;
using QL.Core.Attributes;

namespace QL.Actions.Standard.CurrentTime;

public record CurrentTimeArguments;

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
     * AM or PM as a string
     */
    public string AmPm { get; set; }
    
    /**
     * Time zone as a string (e.g. "PST", "MST", "EST", etc.)
     */
    public string TimeZone { get; set; }
    
    /**
     * Year as an integer
     */
    public int Year { get; set; }
    
    /**
     * Current time as a DateTime object (does not include TimeZone)
     */
    public DateTime DateTime
    {
        get
        {
            try
            {
                var dateTimeString = $"{Month} {Day} {Year} {Time} {AmPm}";
                return DateTime.ParseExact(dateTimeString, "MMM d yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
            }
            catch (FormatException ex)
            {
                // Handle the exception as needed, for example, log it or throw a new exception
                throw new InvalidOperationException("Failed to parse DateTime.", ex);
            }
        }
    }
}

[Action]
[Cmd("date")]
[Regex(@"(?<dayOfWeek>\w{3})\s+(?<month>\w{3})\s+(?<day>\d{1,2})\s+(?<time>\d{2}:\d{2}:\d{2})\s+(?<AMPM>AM|PM)\s+(?<timeZone>\w{3})\s+(?<year>\d{4})")]
public class CurrentTime : ActionBase<CurrentTimeArguments, CurrentTimeResult>;
