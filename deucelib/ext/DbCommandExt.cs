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
    public static DbParameter CreateWithValue(this DbCommand command, string parameterName, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Value = value;
        return parameter;
    }
}