using System.Diagnostics;
using deuce;
using MySql.Data.MySqlClient;

namespace deuce_unit;

[TestClass]
public class UnitTestsBuilderSchedule
{

    [TestMethod]
    public async Task build_schedule_for_tournament()
    {
        //Assign
        Organization club = new Organization() { Id = 1 };
        //Make a random tournament
        AssignTournament tourRepo = new();
        Tournament tournament = await tourRepo.MakeRandom(1, "testing_tournament", 8, 1, 2, 2, 1, 2);
        //Open db connection
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        //Save the tournament
        DbRepoTournament dbRepoTour = new(conn, club);

        await dbRepoTour.SetAsync(tournament);

        //Save teams
        DbRepoTeam dbRepoTeam= new(conn, club, tournament.Id);
        foreach(Team iterTeam in tournament.Teams??new List<Team>())
             await dbRepoTeam.SetAsync(iterTeam);
        //Save schedule



        var dbrepo = new DbRepoRecordSchedule(conn);
        //Load schedule for tournament 3 , tournament must exist
        List<RecordSchedule> recordsSched = await dbrepo.GetList(new Filter() { TournamentId = tournament.Id });

        //Action
        try
        {
            //List of players and teams for this tournament
            var dbrepotp = new DbRepoRecordTeamPlayer(conn);
            List<RecordTeamPlayer> teamplayers = await dbrepotp.GetList(new Filter() { TournamentId = tournament.Id });

            PlayerRepo playerRepo = new PlayerRepo();
            TeamRepo teamRepo = new TeamRepo(teamplayers);

            List<Player> players = playerRepo.ExtractFromRecordTeamPlayer(teamplayers);
            List<Team> teams = teamRepo.ExtractFromRecordTeamPlayer();
            tournament.Teams = teams;

            BuilderSchedule builderSchedule = new BuilderSchedule(recordsSched, players, teams, tournament, conn);
            Schedule schedule = builderSchedule.Create();

            Assert.IsNotNull(schedule, "schedule was null");

            for (int i = 0; i < schedule.NoRounds; i++)
            {
                string filename = tournament.Label + "_round_" + (i + 1).ToString() + ".pdf";
                FileStream pdffile = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
                PdfPrinter printer = new PdfPrinter(schedule);
                printer.Print(pdffile, tournament, schedule, i);
                pdffile.Close();
            }

            Assert.IsTrue(schedule.NoRounds > 0, $"schedule was null");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            //Assert
            Assert.Fail(ex.StackTrace);
        }



    }
}