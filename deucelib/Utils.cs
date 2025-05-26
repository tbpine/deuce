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

    /// <summary>
    /// Get a property value from an object by a dot-separated path.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static object? GetPropertyByPath(object obj, string path)
    {
        int index = path.IndexOf('.');
        if (index < 0 && !string.IsNullOrWhiteSpace(path)) // return the property value
        {
            return obj.GetType().GetProperty(path)?.GetValue(obj);
        }
        //Split the path at the first dot

        string firstPart = path.Substring(0, index);
        string remainingPath = path.Substring(index + 1);

        //Get the property value of the first part
        var firstValue = obj.GetType().GetProperty(firstPart)?.GetValue(obj);

        if (firstValue == null) return null; // If the property is null, return null

        //call GetPropertyByPath recursively on the remaining path
        return GetPropertyByPath(firstValue, remainingPath);
    }
    
    /// <summary>
    /// Set a property value from an object by a dot-separated path.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool SetPropertyByPath(object obj, string path, object? value)
    {
        //Find the string before the first dot
        
        int index = path.IndexOf('.');
        if (index < 0 && !string.IsNullOrWhiteSpace(path)) // return the property value
        {
            obj.GetType().GetProperty(path)?.SetValue(obj, value);
             return true;
        }
        //Split the path at the first dot
        
        string firstPart = path.Substring(0, index);
        string remainingPath = path.Substring(index + 1);

        //Get the property value of the first part
        var firstValue = obj.GetType().GetProperty(firstPart)?.GetValue(obj);

        if (firstValue == null) return false; // If the property is null, return false
        //call GetPropertyByPath recursively on the remaining path
        return SetPropertyByPath(firstValue, remainingPath, value);
    }
}