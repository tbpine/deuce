using System.Diagnostics;
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
        Club club= new Club(){ Id = 1};
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        var dbrepo = new DbRepoRecordSchedule(conn);
        List<RecordSchedule> records = await dbrepo.GetList(new Filter(){TournamentId = tournamentId});
        
        //List of players and teams for this tournament
        var dbrepotp = new DbRepoRecordTeamPlayer(conn);
        List<RecordTeamPlayer> players = await dbrepotp.GetList();
        
        
        
        //Action

        //Assert

    }
}