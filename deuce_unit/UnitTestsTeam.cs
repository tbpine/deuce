namespace deuce_unit;
using deuce.ext;
using deuce;
using MySql.Data.MySqlClient;
using System.Data.Common;

[TestClass]
public class UnitTestsTeam
{
    public UnitTestsTeam() { }

    // [TestMethod]
    [DataRow(50)]
    public void set_n_team_return_teams(int noTeams)
    {
        //Assign
        Club club = new Club(){ Id = 1}        ;

        //Players
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();

        List<Team> teams = new();
        Random r = new();
        for (int i = 0; i < noTeams; i++)
        {
            Team t = new(-1, RandomUtil.GetTeam());
            t.Club = club;

            teams.Add(t);
        }

        //Action
        DbCommand cmd = conn.CreateCommand();
        cmd.CommandText = "sp_set_team";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_id", DBNull.Value));
        cmd.Parameters.Add(cmd.CreateWithValue("p_club", 1));
        cmd.Parameters.Add(cmd.CreateWithValue("p_label", ""));

        for (int i = 0; i < noTeams; i++)
        {
            Team t = teams[i];
            cmd.Parameters["p_club"].Value = t.Club?.Id??1;
            cmd.Parameters["p_label"].Value = t.Label;

            t.Id =(int) (ulong)(cmd.ExecuteScalar()??0L);

        }


        //Assert
    }
}