using System.Diagnostics;
using deuce;
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
        DbConnectionLocal conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        //players , permutation and round, and tournament
        Organization club = new Organization() { Id = 1 };
        try
        {
            conn.Open();
            DbRepoPlayer dbRepoPlayer = new(conn);
            var entries = await dbRepoPlayer.GetList(new Filter() { ClubId = club.Id, TournamentId = 0 });

            //Create tournament
            AssignTournament tourRepo = new();
            Tournament tour =  tourRepo.MakeRandom(1, "test_tournament", 8, 1, 2, 2, 1, 2, entries);
            tour.Id = tournamentId;
            Draw? draw = tour.Draw;

            //Action
            //What is needed to save the tournament for recreation ?
            //Teams
            var dbRepoTeam = new DbRepoTeam(conn)
            {
                Organization = club,
                TournamentId = tour.Id
            };
            
            foreach (Team team in tour.Teams!)
                await dbRepoTeam.SetAsync(team);
            //Save matches
            var dbrepo = new DbRepoMatch(conn);

            for (int i = 0; i < tour.Draw!.NoRounds; i++)
            {
                Round round = tour.Draw.GetRound(i);
                foreach (Permutation p in round.Permutations)
                {
                    foreach (Match match in p.Matches)
                    {
                        if (dbrepo is not null)
                            await dbrepo.SetAsync(match);
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