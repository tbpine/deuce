using System.Data;
using System.Data.Common;
using System.Diagnostics;
using MySql.Data.MySqlClient;

/// <summary>
/// Wrapper around a mysql connection with 
/// the connection string set.
/// </summary>
class DbConnectionLocal : DbConnection
{
    private readonly MySqlConnection _connection;
    private readonly IConfiguration _config;

    //Don't close connection
    private  bool _keepAlive;
    public void KeepAlive(bool value) {_keepAlive = value; }

    public DbConnectionLocal(IConfiguration configuration)
    {
        _connection = new MySqlConnection();
        _config = configuration;
        _connection.ConnectionString = _config.GetConnectionString("deuce_local");
    }

    public override string ConnectionString
    {
        get => _connection.ConnectionString; set => _connection.ConnectionString = value ?? "";
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
        return _connection.BeginTransaction();
    }

    protected override DbCommand CreateDbCommand()
    {
        return _connection.CreateCommand();
    }
}