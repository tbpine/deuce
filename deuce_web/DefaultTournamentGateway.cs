using deuce;
using System.Data.Common;
using System.Diagnostics;
using System.Formats.Tar;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
/// <summary>
/// Load tournament objects from the database
/// </summary>
public class DefaultTournamentGateway : ITournamentGateway
{

    private readonly SessionProxy _sessionProxy;
    private readonly DbRepoTournament _dbRepoTournament;
    private readonly DbRepoTournamentDetail _dbRepoTournamentDetail;
    private readonly DbRepoTournamentStatus _dbRepoTournamentStatus;
    private readonly DbRepoRecordTeamPlayer _dbRepoRecordTeamPlayer;
    private readonly ICacheMaster _cache;
    private readonly DbConnection _dbconn;
    private readonly ILogger<DefaultTournamentGateway> _log;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;
    private readonly DbRepoTeamStanding _dbRepoTeamStanding;



    public DefaultTournamentGateway(SessionProxy sessionProxy, DbRepoTournament dbRepoTournament,
    DbRepoTournamentDetail dbRepoTournamentDetail, ICacheMaster cache, DbConnection dbconn,
    DbRepoTournamentStatus dbRepoTournamentStatus, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer,
    ILogger<DefaultTournamentGateway> log, DbRepoRecordSchedule dbRepoRecordSchedule, 
    DbRepoTeamStanding dbRepoTeamStanding)
    {
        _sessionProxy = sessionProxy;
        _dbRepoTournament = dbRepoTournament;
        _dbRepoTournamentDetail = dbRepoTournamentDetail;
        _cache = cache;
        _dbconn = dbconn;
        _dbRepoTournamentStatus = dbRepoTournamentStatus;
        _dbRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _log = log;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
        _dbRepoTeamStanding = dbRepoTeamStanding;
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
    /// Start the current tournament. Note , the tournament was validated before,
    /// expect a valid sport and enough players. This method will create the schedule for the tournament
    /// and save it to the database. It will also change the tournament status to "started
    /// </summary>
    /// <returns></returns>
    public async Task<ResultTournamentAction> StartTournament(int tournamentId)
    {

        //Load the current tournament and it's details
        var currentTour = await GetTournament(tournamentId);

        if (currentTour is null) return new(ResultStatus.Error, "Unable to start tournament.");

        //check what sports it is
        var listOfSports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        var selectedSport = listOfSports?.Find(e => e.Id == currentTour.Sport);
        
        //Return an error if the selected sport is not found
        if (selectedSport is null) return new(ResultStatus.Error, "Unable to start tournament. Unknown sport.");

        Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1, Name = "" };

        //Get the class that will make games
        //Proceed to create rounds and game
        FactoryGameMaker factoryGameMaker = new FactoryGameMaker();

        IGameMaker gameMaker = factoryGameMaker.Create(selectedSport);

        //If there's no game maker for this sport, return an error
        if (gameMaker is null) return new(ResultStatus.Error, "Unable to start tournament. Don't know how to make games for this sport.");

        //Get the list of teams
        TeamRepo teamRepo = new TeamRepo(currentTour, _dbconn);
        //List of teams always returns a list, even if it's empty. No need to check for null.
        currentTour.Teams = await teamRepo.GetTournamentEntries()??[];
        //------------------------------
        //| Create schedule.
        //------------------------------

        FactoryDrawMaker drawSchedulers = new FactoryDrawMaker();
        var tournamentScheduler = drawSchedulers.Create(currentTour, gameMaker);
        var draw = tournamentScheduler.Create();

        //Check if the schedule is correct
        if (draw is not null)
        {
            currentTour.Draw = draw;
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

    /// <summary>
    /// Build schedule from the database
    /// </summary>
    /// <param name="tournament">The tournament for which to build the schedule</param>
    /// <returns>Draw object representing the schedule</returns>
    private async Task<Draw?> BuildScheduleFromDB(Tournament tournament)
    {
        List<RecordSchedule> recordsSched = await _dbRepoRecordSchedule.GetList(new Filter() { TournamentId = tournament.Id });

        //List of players and teams for this tournament
        List<RecordTeamPlayer> teamplayers = await _dbRepoRecordTeamPlayer.GetList(new Filter() { TournamentId = tournament.Id });

        // Extract teams (which includes players with Team property correctly set)
        TeamRepo teamRepo = new TeamRepo(teamplayers);
        List<Team> teams = teamRepo.ExtractFromRecordTeamPlayer();
        tournament.Teams = teams;

        // Extract players from the teams (these will have their Team property set correctly)
        List<Player> players = teams.SelectMany(t => t.Players).ToList();

        BuilderDraws builderDraw = new BuilderDraws(recordsSched, players, teams, tournament, _dbconn);
        return builderDraw.Create();
    }

    /// <summary>
    /// Progresses the tournament by calling the appropriate DrawMaker's OnChange method.
    /// This handles advancing winners, creating next rounds, and updating the tournament draw.
    /// After progressing the tournament, saves the draw to the database.
    /// </summary>
    /// <param name="tournament">The tournament to progress</param>
    /// <param name="currentRound">The current round that was just completed</param>
    /// <param name="scores">The scores that were just entered</param>
    /// <param name="gameMaker">The game maker for this tournament's sport</param>
    /// <param name="drawMaker">The draw maker for this tournament's format</param>
    /// <returns>ResultTournamentAction indicating success or failure</returns>
    public async Task<ResultTournamentAction> ProgressTournament(Tournament tournament, int currentRound, List<Score> scores, IGameMaker gameMaker, IDrawMaker drawMaker)
    {
        try
        {
            // No progression needed if no scores provided
            if (scores == null || !scores.Any())
            {
                return new(ResultStatus.Warning, "No scores provided for tournament progression.");
            }

            // Ensure we have tournament teams - load if not available
            if (tournament.Teams == null || !tournament.Teams.Any())
            {
                TeamRepo teamRepo = new TeamRepo(tournament, _dbconn);
                List<Team> listOfTeams = await teamRepo.GetListAsync(tournament.Id);
                tournament.Teams = listOfTeams;
            }

            // Ensure we have tournament draw - load from database if not available
            if (tournament.Draw == null)
            {
                tournament.Draw = await BuildScheduleFromDB(tournament);
                if (tournament.Draw == null)
                {
                    _log.LogWarning("Cannot progress tournament {TournamentId} - unable to load draw from database", tournament.Id);
                    return new(ResultStatus.Error, "Unable to load tournament draw from database.");
                }
            }

            // Progress the tournament by calling OnChange
            // This will advance winners, create next rounds, etc. based on the tournament type
            drawMaker.OnChange(tournament.Draw, currentRound, currentRound - 1, scores);

            // Save the updated draw to the database
            Organization thisOrg = new() { Id = _sessionProxy?.OrganizationId ?? 1, Name = "" };
            TournamentRepo tourRepo = new(_dbconn, tournament, thisOrg);
            bool scheduleSaved = await tourRepo.SaveSchedule();

            if (scheduleSaved)
            {
                // Save the updated standings to the database
                var currentStandings = tournament.GetCurrentStandings();
                if (currentStandings != null && currentStandings.Any())
                {
                    // Set the tournament ID for each standing record to ensure proper database linking
                    foreach (var standing in currentStandings)
                    {
                        standing.Tournament = tournament.Id;
                    }
                    
                    try
                    {
                        await _dbRepoTeamStanding.Sync(currentStandings);
                        _log.LogInformation("Tournament progressed and standings saved for tournament {TournamentId}, round {Round} with {StandingsCount} team standings",
                                          tournament.Id, currentRound, currentStandings.Count);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex, "Failed to save tournament standings after progression for tournament {TournamentId}", tournament.Id);
                        return new(ResultStatus.Warning, "Tournament progressed and draw saved, but failed to save standings to database.");
                    }
                }
                
                _log.LogInformation("Tournament progressed and saved for tournament {TournamentId}, round {Round} with {ScoreCount} scores",
                                  tournament.Id, currentRound, scores.Count);
                return new(ResultStatus.Ok, "Tournament progressed and draw saved successfully.");
            }
            else
            {
                _log.LogError("Failed to save tournament draw after progression for tournament {TournamentId}", tournament.Id);
                return new(ResultStatus.Error, "Tournament progressed but failed to save draw to database.");
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error progressing tournament {TournamentId} for round {Round}",
                         tournament.Id, currentRound);
            return new(ResultStatus.Error, $"Error progressing tournament: {ex.Message}");
        }
    }
}
