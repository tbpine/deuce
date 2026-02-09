//The whole point of MVC is have the controller handle actions
//Pass URI parameters as action data
//Or post.

using Microsoft.AspNetCore.Mvc;
using deuce;
using System.Data.Common;
using System.Diagnostics;
public class ScoringController : MemberController
{

    private readonly DbRepoRecordTeamPlayer _deRepoRecordTeamPlayer;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;
    private readonly DbConnection _dbConnection;
    private readonly ILogger<ScoringController> _log;
    private readonly ICacheMaster _cache;
    private readonly DbRepoTeamStanding _dbRepoTeamStanding;

    private readonly IScoreKeeper _scoreKeeper;

    /// <summary>
    /// Scoring controller for managing scores in tournaments.
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
    /// <param name="dbRepoTeamStanding">DbRepo team standing</param>
    public ScoringController(ILogger<ScoringController> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbConnection dbConnection,
    DbRepoRecordSchedule dbRepoRecordSchedule, ICacheMaster cache, IScoreKeeper scorekeeper, DbRepoTeamStanding dbRepoTeamStanding) : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _deRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
        _dbConnection = dbConnection;
        _cache = cache;
        _scoreKeeper = scorekeeper;
        _dbRepoTeamStanding = dbRepoTeamStanding;
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
            await LoadScore();
        }
        catch (Exception ex)
        {
            // Log the error and return an error view
            _log.LogError(ex, "Error retrieving tournament details for ID {TournamentId}", _model.Tournament.Id);
        }

        // Load the scores for the current tournament
        // Set the title for the page
        _model.Title = "Scoring";

        //Get tournament type from the cache
        var tournamentTypes = await _cache.GetList<TournamentType>(CacheMasterDefault.KEY_TOURNAMENT_TYPES);
        //Find the KO tournament type
        var tournamentType = tournamentTypes?.FirstOrDefault(tt => tt.Key == "ko");
        //Load "KO.cshtml" view model if the tournament type is knockout
        string viewName = _model.Tournament.Type == (tournamentType?.Id ?? -1) ? "KO" : "Index";

        // Return the view with the model
        return View(viewName, _model);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRound(int round)
    {
        //Save current round first
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");

        //Get the cuurent round from the model

        int currentRound = _model.CurrentRound;
        //Use FormReaderScoring to parse the form data
        FormReaderScoring formReader = new FormReaderScoring();
        //parse scores from the form
        var formScores = formReader.Parse(this.Request.Form, _model.Tournament.Id);

        //Use proxy to save scores for this tournament at the current roud
        await _scoreKeeper.Save(_sessionProxy?.TournamentId ?? 0, formScores, currentRound, _dbConnection);

        //Change the round in the model
        _model.CurrentRound = round;
        //And the session proxy
        if (_sessionProxy is not null) _sessionProxy.CurrentRound = round;
        //Reload the scores for the new round
        await LoadScore();

        return View("Index", _model);

    }

    [HttpPost]
    public async Task<IActionResult> Save()
    {
        // Save scores for the current round.
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");
        try
        {
            // Get the current round from the model
            int currentRound = _model.CurrentRound;
            // Use FormReaderScoring to parse the form data
            FormReaderScoring formReader = new FormReaderScoring();
            // Parse scores from the form
            var formScores = formReader.Parse(this.Request.Form, _model.Tournament.Id);
            // Use proxy to save scores for this tournament at the current round
            await _scoreKeeper.Save(_model.Tournament.Id, formScores, currentRound, _dbConnection);

            //If the score changed, then progress the tournament by
            //calling the "OnChange" method of the draw maker:
            await ProgressTournament(currentRound, formScores);

            // Reload the scores for the new round
            await LoadScore();
        }
        catch (Exception ex)
        {
            _log.LogCritical(ex.Message);
        }

        return View("Index", _model);

    }

    public async Task<IActionResult> Print(int round, bool show_scores)
    {
        //Get the current tournament from the gateway

        var tournament = await _tourGateway.GetCurrentTournament();
        int tourId = _model.Tournament.Id;
        int currentRound = _model.CurrentRound;

        //call the builderSchedulefromdb method to get the schedule from the database
        //and build the schedule object
        //Get tournament details 
        var schedule = await BuildScheduleFromDB();

        List<Score>? formScores = null;
        //Get scores for the current round
        //from the form values
        if (show_scores)
        {

            // FormReaderScoring formReader = new FormReaderScoring();
            // formScores = formReader.Parse(this.Request.Form, _sessionProxy?.TournamentId ?? 0);
            //Get the scores from the database
            formScores = await ProxyScores.GetScores(tourId, currentRound, _dbConnection);
        }

        _model.Tournament = tournament;
        _model.Draw = schedule;
        _model.CurrentRound = currentRound;
        _model.Tournament.Draw = schedule;

        //Load Page and return if the schedule is null
        if (schedule is null || tournament is null)
        {
            await LoadScore();
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
            await pdfPrinter.Print(this.HttpContext.Response.BodyWriter.AsStream(true), tournament, schedule, currentRound,
            formScores);
        }
        catch (Exception ex)
        {
            _log.LogError(ex.Message);
        }

        return View("Index", _model);

    }

    public async Task<IActionResult> Reload()
    {
        await LoadScore();
        return View("Index", _model);
    }



    /// <summary>
    /// Load the scores for the current tournament from the database.
    /// </summary>
    /// <returns></returns>
    private async Task LoadScore()
    {
        //Get the current tournament
        //DB access
        _model.Tournament = await _tourGateway.GetTournament(_model.Tournament.Id);

        //Extract teams and players
        TeamRepo teamRepo = new TeamRepo(_model.Tournament, _dbConnection);
        List<Team> listOfTeams = (await teamRepo.GetListAsync(_model.Tournament.Id));

        //TODO: This is incorrect.
        //The schedule should be built from
        //rows of the match and match_player tables
        //respectively.
        //Get the schedule from the database
        var schedule = await BuildScheduleFromDB();
        _model.Tournament.Draw = schedule;
        _model.Draw = schedule;

        //Get scores using  ProxyScores method GetScores
        List<Score> listOfScores = await ProxyScores.GetScores(_model.Tournament.Id, _model.CurrentRound, _dbConnection);
        //use LINQ t6o filter scores by round order by permutation and set
        _model.RoundScores = listOfScores.Where(s => s.Round == _model.CurrentRound).OrderBy(s => s.Permutation).ThenBy(s => s.Match).ThenBy(s => s.Set).ToList();



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

    /// <summary>
    /// Progresses the tournament by calling the appropriate DrawMaker's OnChange method.
    /// This handles advancing winners, creating next rounds, and updating the tournament draw.
    /// </summary>
    /// <param name="currentRound">The current round that was just completed</param>
    /// <param name="scores">The scores that were just entered</param>
    /// <returns></returns>
    private async Task ProgressTournament(int currentRound, List<Score> scores)
    {
        try
        {
            // No progression needed if no scores provided
            if (scores == null || !scores.Any())
            {
                return;
            }

            // Ensure we have full tournament details - load from database if needed
            if (_model.Tournament.Id == 0)
                _model.Tournament = await _tourGateway.GetTournament(_model.Tournament.Id);

            // Ensure we have tournament teams - load if not available
            if (_model.Tournament.Teams == null)
            {
                TeamRepo teamRepo = new TeamRepo(_model.Tournament, _dbConnection);
                List<Team> listOfTeams = await teamRepo.GetListAsync(_model.Tournament.Id);
                _model.Tournament.Teams = listOfTeams;
            }

            // Ensure we have tournament draw - load from database if not available
            if (_model.Tournament.Draw == null)
            {
                _model.Tournament.Draw = await BuildScheduleFromDB();
                if (_model.Tournament.Draw == null)
                {
                    _log.LogWarning("Cannot progress tournament - unable to load draw from database");
                    return;
                }
            }

            // Create game maker (tennis is the standard)
            FactoryGameMaker gameFactory = new FactoryGameMaker();
            IGameMaker gameMaker = gameFactory.Create(new Sport(_model.Tournament.Sport, "Tennis", "", "", ""));

            // Create the appropriate draw maker based on tournament type
            FactoryDrawMaker drawFactory = new FactoryDrawMaker();
            IDrawMaker drawMaker = drawFactory.Create(_model.Tournament, gameMaker);

            // Progress the tournament by calling OnChange
            // This will advance winners, create next rounds, etc. based on the tournament type
            drawMaker.OnChange(_model.Tournament.Draw, currentRound + 1, currentRound, scores);

            _log.LogInformation("Tournament progressed for tournament {TournamentId}, round {Round} with {ScoreCount} scores", 
                              _model.Tournament.Id, currentRound, scores.Count);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error progressing tournament {TournamentId} for round {Round}", 
                         _model.Tournament.Id, currentRound);
        }
    }



}