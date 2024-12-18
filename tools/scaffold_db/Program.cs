using MySql.Data.MySqlClient;

MySqlConnection dbConn = new MySqlConnection();
MySqlConnectionStringBuilder sb = new();
sb.Server = "localhost";
sb.UserID = "deuce";
sb.Password = "deuce";
sb.Database = "deuce";

dbConn.ConnectionString = sb.ToString();

dbConn.Open();
var dtTbl = await dbConn.GetSchemaAsync("Tables");
var dtCols = await dbConn.GetSchemaAsync("Columns");

dtTbl.WriteXml("tables.xml");
dtCols.WriteXml("cols.xml");

ScaffoldSelect scaffoldSelect= new ScaffoldSelect();
scaffoldSelect.MakeSql("procs.sql");
scaffoldSelect.MakeSql("drop_procs.sql",true);

dbConn.Close();

