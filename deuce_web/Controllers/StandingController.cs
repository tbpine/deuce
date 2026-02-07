//Standing controller for displaying Swiss format tournament standings
//Shows current standings for each round with navigation between rounds

using Microsoft.AspNetCore.Mvc;
using deuce;
using System.Data.Common;
using System.Diagnostics;

public class StandingController : MemberController
{

    private readonly DbRepoRecordTeamPlayer _deRepoRecordTeamPlayer;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;
    private readonly DbConnection _dbConnection;
    private readonly ILogger<StandingController> _log;
    private readonly ICacheMaster _cache;

    /// <summary>
    /// Standing controller for displaying standings in Swiss format tournaments.
    /// 
    /// </summary>
    /// <param name="log">Log</param>
    /// <param name="handlerNavItems">Member menus</param>
    /// <param name="sp">Service provider</param>
    /// <param name="config">Configurator</param>
    /// <param name="tgateway">Tournament gateway</param>
    /// <param name="sessionProxy">Session proxy</param>
    /// <param name="dbRepoRecordTeamPlayer">DbRepo team player</param>
    /// <param name="dbConnection">DbConnection</param>
    /// <param name="dbRepoRecordSchedule">DbRepo record schedule</param>
    /// <param name="cache">Cache master</param>
    public StandingController(ILogger<StandingController> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbConnection dbConnection,
    DbRepoRecordSchedule dbRepoRecordSchedule, ICacheMaster cache) : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _deRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
        _dbConnection = dbConnection;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int tournament)
    {
        // Set the tournament ID in the model, not from session
        _model.Tournament.Id = tournament;
        _sessionProxy.TournamentId = tournament;
        // Set the current round to 0
        _model.CurrentRound = 0;
        _sessionProxy.CurrentRound = 0;
        try
        {
            await LoadStandings();
        }
        catch (Exception ex)
        {
            // Log the error and return an error view
            _log.LogError(ex, "Error retrieving tournament standings for ID {TournamentId}", _model.Tournament.Id);
        }

        // Set the title for the page
        _model.Title = "Standings";

        // Return the view with the model
        return View("Index", _model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRound(int round)
    {
        //Change the round in the model
        _model.CurrentRound = round;
        //And the session proxy
        if (_sessionProxy is not null) _sessionProxy.CurrentRound = round;
        //Reload the standings for the new round
        await LoadStandings();

        return View("Index", _model);
    }

    public async Task<IActionResult> Print(int round)
    {
        //Get the current tournament from the gateway
        var tournament = await _tourGateway.GetCurrentTournament();
        int tourId = _model.Tournament.Id;
        int currentRound = _model.CurrentRound;

        //Get tournament details 
        var schedule = await BuildScheduleFromDB();

        _model.Tournament = tournament;
        _model.Draw = schedule;
        _model.CurrentRound = currentRound;
        _model.Tournament.Draw = schedule;

        //Load Page and return if the schedule is null
        if (schedule is null || tournament is null)
        {
            await LoadStandings();
            return View("Index", _model);
        }

        //Use the PdfPrinter class to generate the PDF
        PdfPrinter pdfPrinter = new PdfPrinter(schedule, new PDFTemplateFactory());
        //Get the response output stream
        //Send the PDF in the response.
        this.Response.ContentType = "application/pdf";

        //Call the print method to generate the PDF
        try
        {
            await pdfPrinter.Print(this.HttpContext.Response.BodyWriter.AsStream(true), tournament, schedule, currentRound, null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return View("Index", _model);
    }

    public async Task<IActionResult> Reload()
    {
        await LoadStandings();
        return View("Index", _model);
    }

    /// <summary>
    /// Load the standings for the current tournament from the database.
    /// </summary>
    /// <returns></returns>
    private async Task LoadStandings()
    {
        //Get the current tournament
        //DB access
        _model.Tournament = await _tourGateway.GetTournament(_model.Tournament.Id);

        //Extract teams and players
        TeamRepo teamRepo = new TeamRepo(_model.Tournament, _dbConnection);
        _model.Tournament.Teams = (await teamRepo.GetListAsync(_model.Tournament.Id));
         
        //Get the schedule from the database
        var schedule = await BuildScheduleFromDB();
        _model.Tournament.Draw = schedule;
        _model.Draw = schedule;
        //Game maker
        FactoryGameMaker factoryGameMaker = new FactoryGameMaker();
        //Get a list of sports from the cache
        var listOfSports = await _cache.GetList<Sport>(CacheMasterDefault.KEY_SPORTS);
        var sport = listOfSports?.FirstOrDefault(s => s.Id == _model.Tournament.Sport);

        var facGameMaker = new FactoryGameMaker();
        var gameMaker = facGameMaker.Create(sport);
        //Draw maker
        FactoryDrawMaker factoryDraws = new FactoryDrawMaker();
        var drawMaker = factoryDraws.Create( _model.Tournament, gameMaker);
        //Get the standings for the current round from the tournament
        

        _model.TeamStandings = _model.Tournament.GetStandingsForRound(_model.CurrentRound)??[];


    }

    //Build schedule from the database
    private async Task<Draw?> BuildScheduleFromDB()
    {
        List<RecordSchedule> recordsSched = await _dbRepoRecordSchedule.GetList(new Filter() { TournamentId = _model.Tournament.Id });

        //List of players and teams for this tournament
        var dbrepotp = new DbRepoRecordTeamPlayer(_dbConnection);
        List<RecordTeamPlayer> teamplayers = await dbrepotp.GetList(new Filter() { TournamentId = _model.Tournament.Id });

        PlayerRepo playerRepo = new PlayerRepo();
        TeamRepo teamRepo = new TeamRepo(teamplayers);

        List<Player> players = playerRepo.ExtractFromRecordTeamPlayer(teamplayers);
        List<Team> teams = teamRepo.ExtractFromRecordTeamPlayer();
        _model.Tournament.Teams = teams;

        BuilderDraws builderDraw = new BuilderDraws(recordsSched, players, teams, _model.Tournament, _dbConnection);
        return builderDraw.Create();
    }
}