using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;

/// <summary>
/// Wrapper around a mysql connection with 
/// the connection string set.
/// </summary>
class DbConnectionLocal : DbConnection
{
    private readonly MySqlConnection _connection;

    //Don't close connection
    private  bool _keepAlive;
    /// <summary>
    /// If true, there's an existing transaction on the connection.
    /// </summary>
    public void KeepAlive(bool value) {_keepAlive = value; }

    public DbConnectionLocal(string connStr)
    {
        _connection = new MySqlConnection();
        _connection.ConnectionString = connStr;
    }

    [AllowNull]
    public override string ConnectionString
    {
        get => _connection.ConnectionString??"";
        set  {
             _connection.ConnectionString = value;
             }
    }


    public override string Database => _connection.Database;

    public override string DataSource => _connection.DataSource;

    public override string ServerVersion => _connection.ServerVersion;

    public override ConnectionState State => _connection.State;

    public override void ChangeDatabase(string databaseName)
    {
        _connection.ChangeDatabase(databaseName);
    }

    public override void Close()
    {
        //If you want to reuse in multi queries.
        if (!_keepAlive)
        {
            _connection.Close();
            Debug.WriteLine($"DBCONN:{this.GetHashCode()}|closed.");
        }
    }

    public override void Open()
    {
        //Close broken connections
        if (_connection.State == ConnectionState.Broken) _connection.Close();
        //Connection is open already or is busy
        if (_connection.State == ConnectionState.Open || 
                _connection.State == ConnectionState.Connecting ||
                _connection.State == ConnectionState.Executing ||
                _connection.State == ConnectionState.Fetching) return;
        //Existing transactions
        //and reader.
               
          _connection.Open();
          

        Debug.WriteLine($"DBCONN:{this.GetHashCode()}|opened.");
    }


    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        return  _connection.BeginTransaction();;
    }

    protected override DbCommand CreateDbCommand()
    {
        return _connection.CreateCommand();
    }
}