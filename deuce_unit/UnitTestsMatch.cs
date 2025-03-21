using System.Diagnostics;
using deuce;
using deuce.ext;
using MySql.Data.MySqlClient;

namespace deuce_unit;

[TestClass]
public class UnitTestsMatch
{

    [TestMethod]
    public async Task set_match_returns_id()
    {
        //Assign
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        //players , permutation and round, and tournament
        //Action

        try
        {
            conn.Open();
            //Create tournament
            AssignTournament tourRepo = new();
            Tournament tour = tourRepo.MakeRandom(1, "UnitTestsMatch test tour", 8, 1, 1, 0,1, 1);
            Schedule? schedule = tour.Schedule;

            Assert.IsNotNull(schedule, "Tournment has no schedule");
            Debug.WriteLine($"No of rounds = {schedule.NoRounds}");
            //Save tournament, but not details and teams
            Organization org = new() { Id = 1, Name = "test org"};
            DbRepoTournament dbRepoTournament = new(conn, org);
            await dbRepoTournament.SetAsync(tour);

            //Data storage
            var dbrepo = new DbRepoMatch(conn);

            for (int i = 0; i < schedule.NoRounds; i++)
            {
                Round round =  schedule.GetRoundAtIndex(i);
                foreach(Permutation p in round.Permutations)
                {
                    foreach(Match match in p.Matches)
                    {
                        if(dbrepo is not null)
                            await dbrepo.SetAsync(match);
                    }
                }
            }

            //Assert
            for (int i = 0; i < schedule.NoRounds; i++)
            {
                Round round =  schedule.GetRoundAtIndex(i);
                foreach(Permutation p in round.Permutations)
                {
                    foreach(Match match in p.Matches)
                    {
                        Assert.IsTrue(match.Id>0, $"match did not save. {match.GetTitle()}");
                    }
                }
            }




        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Assert.Fail(ex.Message);
        }
        //Assert
    }
}