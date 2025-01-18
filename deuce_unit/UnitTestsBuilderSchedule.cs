using System.Diagnostics;
using System.Xml.Serialization;
using deuce;
using deuce.ext;
using deuce.lib;
using MySql.Data.MySqlClient;

namespace deuce_unit;

[TestClass]
public class UnitTestsBuilderSchedule
{

    [TestMethod]
    [DataRow(3)]
    public async Task build_schedule_for_tournament(int tournamentId)
    {
        //Assign
        Organization club = new Organization() { Id = 1 };
        Tournament tournament = new Tournament() { Id = tournamentId, Label = "test", Sport =1};
        tournament.Format = new Format(2, 2, 1);

        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        await conn.OpenAsync();
        var dbrepo = new DbRepoRecordSchedule(conn);
        List<RecordSchedule> recordsSched = await dbrepo.GetList(new Filter() { TournamentId = tournamentId });

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