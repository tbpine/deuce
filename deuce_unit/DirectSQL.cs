using MySql.Data.MySqlClient;

class DirectSQL
{
    public static void Run(string command)
    {
        using (MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;"))
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = command;
            cmd.CommandType = System.Data.CommandType.Text;
            cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
        }

    }
}