using System.Data.Common;

namespace deuce.ext;
/// <summary>
/// Extra DbCommand methods to add parameters
/// for a stored procedure
/// </summary>
public static class DbCommandExt
{
    /// <summary>
    /// Create a parameter with name and value
    /// </summary>
    /// <param name="command">DbCommand object</param>
    /// <param name="parameterName">Name of parameter</param>
    /// <param name="value">Parameter value</param>
    /// <returns>A DbPrameter object</returns>
    public static DbParameter CreateWithValue(this DbCommand command, string parameterName, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;

        //Change empty strings to null
        if (value is null)
            parameter.Value = DBNull.Value;
        else if (value is string && string.IsNullOrEmpty(value?.ToString()??""))
            parameter.Value = DBNull.Value;
        else
            parameter.Value = value;

        return parameter;
    }

    /// <summary>
    /// Returns a integer from a database column
    /// </summary>
    /// <param name="command">DbCommand object</param>
    /// <param name="scaler">the value</param>
    /// <returns>A DbPrameter object</returns>
    public static int GetIntegerFromScaler(this DbCommand command, object? scaler)
    {
        //handle nulls
        if (scaler is null) return default(int);

        //Check what type of object "scaler" is
        //and return the native type int
        //by mutli-casting

        if (scaler is long) return (int)(long)scaler;
        else if (scaler is ulong) return (int)(ulong)scaler;
        else if (scaler is short ) return (short)scaler;
        else if (scaler is ushort ) return (ushort)scaler;
        else 
        {
            int i = 0;
            try { i = (int)scaler;}catch{ return default(int); }
            return i;
        }

        
    }

}