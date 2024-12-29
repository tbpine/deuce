namespace deuce_unit;
using deuce.ext;
using deuce;
using MySql.Data.MySqlClient;


[TestClass]
public class UnitTestsTeam
{
    public UnitTestsTeam() { }

    [TestMethod]
    [DataRow(4, 2)]
    public void set_n_team_return_teams(int noTeams, int teamSize)
    {
        //Assign
        Club club = new Club(){ Id = 1}        ;

        //Players
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "sp_get_player";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("p_club_id", DBNull.Value);

        var reader = cmd.ExecuteReader();
        List<Player> players = new();
        while (reader.Read())
        {
            Player p = new()
            {
                Id = reader.Parse<int>("id"),
                First = reader.Parse<string>("first_name"),
                Last = reader.Parse<string>("last_name"),
                Ranking = reader.Parse<double>("utr"),
                Club = club
            };

            players.Add(p);

        }

        reader.Close();

        List<Team> teams = new();
        Random r = new();
        for (int i = 0; i < noTeams; i++)
        {
            Team t = new(-1, RandomUtil.GetTeam());
            t.Club = club;

            for (int j = 0; j < teamSize; j++)
            {
                var p = players[r.Next() % players.Count];
                t.AddPlayer(p);
                players.Remove(p);
            }

            teams.Add(t);
        }

        //Action
        cmd = conn.CreateCommand();
        cmd.CommandText = "sp_set_team";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("p_id", DBNull.Value);
        cmd.Parameters.AddWithValue("p_club", 1);
        cmd.Parameters.AddWithValue("p_label", "");

        for (int i = 0; i < noTeams; i++)
        {
            Team t = teams[i];
            cmd.Parameters["p_club"].Value = t.Club?.Id??1;
            cmd.Parameters["p_label"].Value = t.Label;

            t.Id = (int)((ulong)cmd.ExecuteScalar());

            var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "sp_set_team_player";
            cmd2.CommandType = System.Data.CommandType.StoredProcedure;
            cmd2.Parameters.AddWithValue("p_team", DBNull.Value);
            cmd2.Parameters.AddWithValue("p_player", DBNull.Value);
            cmd2.Parameters.AddWithValue("p_club", 1);

            foreach (Player p in t.Players)
            {

                cmd2.Parameters["p_team"].Value = t.Id;
                cmd2.Parameters["p_player"].Value = p.Id;
                
                cmd2.ExecuteNonQuery();
            }
        }


        //Assert
    }
}