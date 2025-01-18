namespace deuce.ext;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Org.BouncyCastle.Asn1.X509.Qualified;

/// <summary>
/// Make specific command objects
/// </summary>
public static class DbDataConnectionExt
{

    /// <summary>
    /// Create a store proc command with parameters
    /// </summary>
    /// <param name="conn">Database connection</param>
    /// <param name="procName">Procedure name</param>
    /// <param name="paramNames">Parameter names</param>
    /// <param name="paramValues">Parameter values</param>
    /// <param name="transaction">Any transaction</param>
    /// <returns>DbCommand object</returns>
    /// <exception cref="ArgumentException"></exception>
    public static DbCommand CreateCommandStoreProc(this DbConnection conn, string procName,
    string[] paramNames, object[] paramValues, DbTransaction? transaction = null)
    {
        //Create a store procedure command
        //set type and commmand text,
        //optionally start a transaction
        var command = conn.CreateCommand();
        command.CommandText = procName;
        command.CommandType = System.Data.CommandType.StoredProcedure;
        if (transaction is not null) command.Transaction = transaction;

        //Check that there's enough parameter values
        if (paramNames.Length != paramValues.Length) throw new ArgumentException("Mismatch parameter values and names");

#if DEBUG
        StringBuilder sbProcCall = new($"call {procName}(");
#endif
        //Add parameters
        for (int i = 0; i < paramNames.Length; i++)
        {
            command.Parameters.Add(command.CreateWithValue(paramNames[i], paramValues[i]));
#if DEBUG
            if (paramValues[i] is string) sbProcCall.Append($"'{paramValues[i]}'");
            sbProcCall.Append($"{paramValues[i]}");
            if (i < paramNames.Length - 1) sbProcCall.Append(",");
#endif
        }

#if DEBUG
            sbProcCall.Append(");");
            Debug.WriteLine(sbProcCall.ToString());
#endif
        return command;
    }


}