using System.Diagnostics;
using deuce;
using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using MySql.Data.MySqlClient;

namespace deuce_unit;

[TestClass]
public class UnitTestsBuilderSchedule
{

    [TestMethod]
    [DataRow(1, 2, 2, 2, 8)]
    public async Task build_schedule_for_rr(int tourType, int noSingle, int noDouble, int teamSize,
    int noPlayers)
    {
        Organization orgainization = new Organization() { Id = 1 };
        DbConnectionLocal conn = new DbConnectionLocal("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();

        //Open db connection
        //Get a tournament id
        //Save players for FKs
        DbRepoPlayer dbRepoPlayer = new(conn);
        var entries = await dbRepoPlayer.GetList(new Filter() { ClubId = orgainization.Id, TournamentId = 0 });

        //Assign
        //Make a random tournament
        AssignTournament assignTour = new();
        Tournament tournament = assignTour.MakeRandom(tourType, "testing_tournament", noPlayers, 1, noSingle, noDouble,
         1, teamSize, entries);


        //Save the tournament using the tour repo
        //class. Saves teams as well.
        TournamentRepo tourRepo = new(conn, tournament, orgainization);
        await tourRepo.Save();

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
                await printer.Print(pdffile, tournament, schedule, i);
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

    [TestMethod]
    [DataRow(2, 1, 0, 1, 7)]
    [DataRow(2, 1, 0, 1, 15)]
    [DataRow(2, 1, 0, 1, 31)]
    public async Task build_schedule_for_ko(int tourType, int noSingle, int noDouble, int teamSize,
    int noPlayers)
    {
        Organization orgainization = new Organization() { Id = 1 };
        DbConnectionLocal conn = new DbConnectionLocal("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();

        //Open db connection
        //Get a tournament id
        //Save players for FKs
        DbRepoPlayer dbRepoPlayer = new(conn);
        var entries = await dbRepoPlayer.GetList(new Filter() { ClubId = orgainization.Id, TournamentId = 0 });

        //Assign
        //Make a random tournament
        AssignTournament assignTour = new();
        Tournament tournament = assignTour.MakeRandom(tourType, "testing_tournament", noPlayers, 1, noSingle, noDouble,
         1, teamSize, entries);


        //Save the tournament using the tour repo
        //class. Saves teams as well.
        TournamentRepo tourRepo = new(conn, tournament, orgainization);
        await tourRepo.Save();

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