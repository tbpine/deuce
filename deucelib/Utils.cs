namespace deuce;

/// <summary>
/// Some shared methods.
/// </summary>
public class Utils
{
    /// <summary>
    /// Get number of days for an interval
    /// </summary>
    /// <param name="interval">Daily, Weekly...</param>
    /// <returns>No of days</returns>
    public static int GetNoDays(Interval interval)
    {
        switch (interval.Id)
        {
            case 4 : return 7;
            case 5 : return 14;
            default: return 1;
        }
    }
}