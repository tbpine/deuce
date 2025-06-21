using deuce;
using System.Data.Common;
using System.Diagnostics;
using System.Formats.Tar;
using System.Threading.Tasks;
/// <summary>
/// Load tournament objects from the database
/// </summary>
public class DBTournamentGateway : ITournamentGateway
{

    private readonly SessionProxy _sessionProxy;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoTournamentStatus _dbRepoTournamentStatus;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly ICacheMaster _cache;
    private readonly DbConnection _dbconn;



    public DBTournamentGateway(SessionProxy sessionProxy, DbRepoTournament dbRepoTournament,
    DbRepoTournamentDetail dbRepoTournamentDetail, ICacheMaster cache, DbConnection dbconn,
    DbRepoTournamentStatus dbRepoTournamentStatus, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer)
    {
        _sessionProxy = sessionProxy;
        _dbRepoTournament = dbRepoTournament;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _cache = cache;
        _dbconn = dbconn;
        _dbRepoTournamentStatus = dbRepoTournamentStatus;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
    }

    /// <summary>
    /// Load  tournament details from the browser session.
    /// </summary>
    /// <param name="dbconn">Database connectoin</param>
    /// <returns>Tournmanet instance</returns>
    public async Task<Tournament> GetCurrentTournament()
    {


        //Check if there's a tournament saved
        int tourId = _sessionProxy?.TournamentId ?? 0;
        if (tourId < 1) return new();

        Debug.Print($"Select tournament #{tourId}");

        //Load a the current tournament from the database
        //Create a scoped db connection.
        //Use a DBRepo to build the object
        //Select the tournment.Returns in the first element
        //Create filter
        Filter tourFilter = new Filter() { TournamentId = tourId };
        List<Tournament> listOfTour = await _dbRepoTournament.GetList(tourFilter);
        //Close if it was created.
        if (listOfTour.Count  == 0) return new();

        var tournament = listOfTour.First();

        //Get tournament details.

        var tourDetails = (await _dbRepoTournamentDetail.GetList(tourFilter));

        tournament.Details = tourDetails.Count > 0 ? tourDetails.First() : new TournamentDetail();

        return tournament;

    }

    /// <summary>
    /// Load  tournament details from an id
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    /// <returns>Tournmanet instance</returns>
    public async Task<Tournament> GetTournament(int id)
    {


        //Check if there's a tournament saved
        if (id < 1) return new();

        Debug.Print($"Select tournament #{id}");

        //Load a the current tournament from the database
        //Create a scoped db connection.
        //Use a DBRepo to build the object
        //Select the tournment.Returns in the first element
        //Create filter
        Filter tourFilter = new Filter() { TournamentId = id };
        List<Tournament> listOfTour = await _dbRepoTournament.GetList(tourFilter);
        //Close if it was created.
        if (listOfTour.Count == 0) return new();

        var tournament = listOfTour.First();

        //Get tournament details.

        var tourDetail = await _dbRepoTournamentDetail.GetList(tourFilter);

        //Get format
        tournament.Details = tourDetail.Count > 0 ? tourDetail.First() : new(); 
           
        return tournament;


    }

    /// <summary>
    /// Start the current tournament
    /// </summary>
    /// <returns></returns>
    public async Task<ResultTournamentAction> StartTournament(int tournamentId)
    {

        //Load the current tournament and it's details
        var currentTour = await GetTournament(tournamentId);

        if (currentTour is null) return new(ResultStatus.Error, "Unable to tournament.");

        //check what sports it is
        var listOfSports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        var selectedSport = listOfSports?.Find(e => e.Id == currentTour.Sport);
        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1, Name = "" };

        //Get the class that will make games
        //Proceed to create rounds and game
        FactoryGameMaker factoryGameMaker = new FactoryGameMaker();

        IGameMaker gameMaker = factoryGameMaker.Create(selectedSport);
        //Get the list of teams
        TeamRepo teamRepo = new TeamRepo(currentTour, _dbconn);
        var listOfTeams = await teamRepo.GetTournamentEntries();

        //------------------------------
        //| Create schedule.
        //------------------------------

        FactorySchedulers facSchedulers = new FactorySchedulers();
        var tournamentScheduler = facSchedulers.Create(currentTour, gameMaker);
        var schedule = tournamentScheduler.Run(listOfTeams ?? new());

        //Check if the schedule is correct
        if (schedule is not null)
        {
            currentTour.Schedule = schedule;
            //Schedule was created for this
            //tournament. Save to the database
            //and change it's status
            TournamentRepo tourRepo = new(_dbconn, currentTour, thisOrg);
            bool scheduleSaved = await tourRepo.SaveSchedule();

            if (scheduleSaved)
            {
                currentTour.Status = TournamentStatus.Start;
                //Save tournament status
                await _dbRepoTournamentStatus.SetAsync(currentTour);
                //Return result status Ok
                return new(ResultStatus.Ok, "Schedule created and saved.");

            }

        }
        else
            return new(ResultStatus.Error, "Could not create schedule for this tournament");

        //Expected error occured

        return new(ResultStatus.Error, "Unknown error.");
    }

    /// <summary>
    /// Check if the current tournament is valid i.e
    /// has enough players and format is defined correctly
    /// </summary>
    /// <returns>ResultTournamentAction instance of the result</returns>
    public async Task<ResultTournamentAction> ValidateCurrentTournament(Tournament theCurrentTour)
    {
        //Check if there's a tournament saved
        int tourId = _sessionProxy?.TournamentId ?? 0;
        if (tourId < 1) return new(ResultStatus.Warning, "Current tournament is unsaved.");

        //Check if the current tournament is valid i.e
        //has enough players and format is defined correctly
        Filter filter = new() { TournamentId = _sessionProxy?.TournamentId??0};

        //Use the dbrepo to get the rows of players 
        var listOfPayers= await _dbRepoRecordTeamPlayer.GetList(filter);

        //Check how many players there are
        if (listOfPayers.Count < 2) return new (ResultStatus.Error, "Not enough players for this tournament!");
                
        //For the sport, validate that the format
        //is correct

        var listOfSports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        if (listOfSports is null)  return new (ResultStatus.Error, "Unknown error.");

        var selectedSport = listOfSports.Find(e=>e.Id == theCurrentTour.Sport);

        if (String.Compare(selectedSport?.Key??"", "tennis") == 0)
        {
            //Tennis format check 
            //Load details
            var tourDetails = (await _dbRepoTournamentDetail.GetList(filter)).FirstOrDefault();
            if ( (tourDetails?.NoSingles??0) == 0 && (tourDetails?.NoDoubles??0) == 0  ) 
                return new(ResultStatus.Error, "You must specify how many single or double matches are played.");
            if ( theCurrentTour.EntryType == (int)EntryType.Team && (tourDetails?.TeamSize??0) == 0)
                return new(ResultStatus.Error, "Team size not specified");

        }

        

        return new(ResultStatus.Ok, "");

    }
}
