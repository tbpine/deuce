namespace deuce;

using System.Data.Common;
using System.Data.SqlTypes;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using deuce;
using Org.BouncyCastle.Crypto.Parameters;

/// <summary>
/// Responsible for the correct create of a 
/// tournament
/// </summary>
public class TournamentOrganizer
{
    private readonly Tournament _tournament;
    private readonly DbConnection _dbConnection;
    private readonly Organization _organization;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="tournament">Tourament to organize</param>
    /// <param name="db">Db Connection</param>
    public TournamentOrganizer(Tournament tournament, DbConnection db, Organization organization)
    {
        _tournament = tournament;
        _dbConnection = db;
        _organization = organization;
    }
    /// <summary>
    /// Given the parameters of a touranment, ensure
    /// that the correct tournament is created i.e
    /// Add the required players  and teams ( get database id 
    /// and create names). For round robin create rounds.
    /// </summary>
    public async Task<ResultSchedule> Run()
    {
        //Validate teams
        //Whos's involved in the tournament
        //Load teams for tournament
        DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer = new(_dbConnection);
        Filter filter = new() { TournamentId = _tournament.Id };
        var recordsOfTeamPlayers = await dbRepoRecordTeamPlayer.GetList(filter);

        //Validation
        if (!Validate(recordsOfTeamPlayers))  return new ResultSchedule(RetCodeScheduling.Error, "Not enough players");

        //Create team structure.
        TeamRepo teamRepo = new(recordsOfTeamPlayers);
        var listOfTeams = teamRepo.ExtractFromRecordTeamPlayer();


        //Get the game maker associated with this sport
        FactoryGameMaker factoryGameMaker = new();
        //DTO sport
        Sport sport = new(_tournament.Sport, "", "", "", "");
        IGameMaker gameMaker = factoryGameMaker.Create(sport);

        //Get the scheduler 
        FactoryDrawMaker factorySchedulers = new();
        var scheduler = factorySchedulers.Create(_tournament, gameMaker);
        _tournament.Teams = listOfTeams;
        scheduler.Create();

        //Save to db
        TournamentRepo tournamentRepo = new(_dbConnection, _tournament, _organization);
        bool isOK = await tournamentRepo.Save();
        //Update status
        if (isOK)
        {

            _tournament.Status = TournamentStatus.Start ;
            DbRepoTournamentStatus depoStatus = new(_dbConnection);
            depoStatus.Set(_tournament);
        }
        else
        {
            return new ResultSchedule(RetCodeScheduling.Error, "Could not save schedule.");
        }

        return new ResultSchedule(RetCodeScheduling.Success, null);

    }

    private bool Validate(List<RecordTeamPlayer> players)
    {
        return players.Count > 1;
    }
}