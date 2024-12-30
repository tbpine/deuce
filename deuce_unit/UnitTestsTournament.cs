using System.Diagnostics;
using deuce;
using deuce.ext;
using deuce.lib;
using MySql.Data.MySqlClient;

namespace deuce_unit;

[TestClass]
public class UnitTestsTournament
{

    [TestMethod]
    [DataRow(3)]
    public async Task set_schedule_returns_id(int tournamentId)
    {
        //Assign
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        //players , permutation and round, and tournament
        Club club = new Club() { Id = 1 };
        try
        {
            conn.Open();
            //Create tournament
            TournamentRepo tourRepo = new();
            Tournament tour = await tourRepo.Random(1, "test_tournament", 8, 1, 2, 2, 1, 2);
            tour.Id = tournamentId;
            Schedule? schedule = tour.Schedule;

            //Action
            //What is needed to save the tournament for recreation ?
            //Teams
            var dbRepoTeam = new DbRepoTeam(conn)
            {
                Club = club,
                Tournament = tour
            };
            foreach (Team team in tour.Teams!)
                await dbRepoTeam.Set(team);
            //Save matches
            var dbrepo = FactoryCreateDbRepo.Create<Match>(conn);

            for (int i = 0; i < tour.Schedule!.NoRounds; i++)
            {
                Round round = tour.Schedule.GetRoundAtIndex(i);
                foreach (Permutation p in round.Permutations)
                {
                    foreach (Match match in p.Matches)
                    {
                        if (dbrepo is not null)
                            await dbrepo.Set(match);
                    }
                }
            }


        }
        catch (Exception ex)
        {
            Debug.Write(ex, ex.Message);
        }




    }
}