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
            case 4: return 7;
            case 5: return 14;
            default: return 1;
        }
    }

    /// <summary>
    /// Splits a full name into first name, middle name, and last name.
    /// </summary>
    /// <param name="fullName">The full name to split.</param>
    /// <returns>An array containing first name, middle name (if any), and last name.</returns>
    public static string[] SplitNames(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return new string[] { "", "", "" }; // Return empty strings if input is null or whitespace
        }

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 1)
        {
            return new string[] { parts[0], "", "" }; // Only first name
        }
        else if (parts.Length == 2)
        {
            return new string[] { parts[0], "", parts[1] }; // First and last name
        }
        else
        {
            return new string[] { parts[0], string.Join(" ", parts[1..^1]), parts[^1] }; // First, middle, and last name
        }
    }
}