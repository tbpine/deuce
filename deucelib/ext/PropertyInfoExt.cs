using System.Reflection;

namespace deuce.ext;


/// <summary>
/// PropertyInfo class extensions.
/// </summary>
public static class PropertyInfoExt
{
    /// <summary>
    /// Returns the string value of a property 
    /// formatted.
    /// </summary>
    /// <param name="propinfo">Property to format</param>
    /// <param name="format">ToString format string</param>
    /// <param name="obj">Object with properties</param>
    /// <returns></returns>
    public static string Format(this PropertyInfo propInfo,object obj,  string? format)
    {
        string retVal = "";
        //Find the property type
        if (propInfo?.PropertyType == typeof(int))
        {
            int? iVal = (int?)propInfo?.GetValue(obj);
            retVal = iVal.HasValue ? !string.IsNullOrEmpty(format) ? iVal.Value.ToString(format) : iVal.Value.ToString() : "";
        }
        else if (propInfo?.PropertyType == typeof(DateTime))
        {
            DateTime? iVal = (DateTime?)propInfo?.GetValue(obj);
            retVal = iVal.HasValue ? !string.IsNullOrEmpty(format) ? iVal.Value.ToString(format) : iVal.Value.ToString() : "";

        }
        else if (propInfo?.PropertyType == typeof(double))
        {
            double? dVal = (double?)propInfo?.GetValue(obj);
            retVal = dVal.HasValue ? !string.IsNullOrEmpty(format) ? dVal.Value.ToString(format) : dVal.Value.ToString()  : "";

        }
        else
            retVal = propInfo?.GetValue(obj)?.ToString() ?? "";
        
        return retVal;
    }

}
