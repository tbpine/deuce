namespace deuce_unit;
using deuce.ext;
using deuce;
using MySql.Data.MySqlClient;
using System.Data.Common;
using deuce.lib;
using System.Diagnostics;

[TestClass]
public class UnitTestsTeam
{
    public UnitTestsTeam() { }

    // [TestMethod]
    [DataRow(50)]
    public void set_n_team_return_teams(int noTeams)
    {
        //Assign
        Organization club = new Organization() { Id = 1 };

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
        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", 1));
        cmd.Parameters.Add(cmd.CreateWithValue("p_label", ""));

        for (int i = 0; i < noTeams; i++)
        {
            Team t = teams[i];
            cmd.Parameters["p_club"].Value = t.Club?.Id ?? 1;
            cmd.Parameters["p_tournament"].Value = 1;
            cmd.Parameters["p_label"].Value = t.Label;

            t.Id = (int)(ulong)(cmd.ExecuteScalar() ?? 0L);

        }


        //Assert
    }

    [TestMethod]
    [DataRow(2, 2, 3)]
    [DataRow(4, 2, 4)]
    public async Task set_team_with_n_players(int noTeams, int noPlayers, int tournamentId)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Id = tournamentId;

        //Players

        Organization club = new Organization() { Id = 1 };
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();

        var dbRepoPlayer = FactoryCreateDbRepo.Create<Player>(conn, club);
        List<Player> players = await dbRepoPlayer!.GetList(new Filter() { ClubId = club.Id });

        var dbRepoTeam = new DbRepoTeam(conn)
        {
            Club = club,
            Tournament = tournament
        };


        Random random = new();
        try
        {
            for (int i = 0; i < noTeams; i++)
            {
                Team team = new Team() { Id = -1, Club = club, Label = RandomUtil.GetTeam() };
                for (int j = 0; j < noPlayers; j++)
                {

                    Player player = players[random.Next() % players.Count];
                    team.AddPlayer(player);
                    players.Remove(player);

                }

                await dbRepoTeam.Set(team);
            }
        }
        catch(Exception ex)
        {
            Debug.Write(ex.Message);
        }


    }
}