//The whole point of MVC is have the controller handle actions
//Pass URI parameters as action data
//Or post.

using Microsoft.AspNetCore.Mvc;
using deuce;


public class ScoringController : MemberedController
{

    private readonly DbRepoRecordTeamPlayer _deRepoRecordTeamPlayer;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;
    private readonly DbConnection _dbConnection;


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
    public ScoringController(ILogger<ScoringController> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbConnection dbConnection,
    DbRepoRecordSchedule dbRepoRecordSchedule)
    {
        base(log, handlerNavItems, sp, config, tgateway, sessionProxy);
        _deRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
        _dbConnection = dbConnection;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int tournamentId)
    {
        // Set the tournament ID in the model
        _model.Tournament.Id = tournamentId;
        SessionProxy.TournamentId = tournamentId;
        // Set the current round to 0
        _model.CurrentRound = 0;
        
        try
        {
            await LoadScore();
        }
        catch (Exception ex)
        {
            // Log the error and return an error view
            _log.LogError(ex, "Error retrieving tournament details for ID {TournamentId}", tournamentId);
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Load the scores for the current tournament
        // Set the title for the page
        _model.Title = "Scoring";

        // Return the view with the model
        return View("Index", _model);
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
        await ScoreKeeper.Save(_sessionProxy?.TournamentId ?? 0, formScores, currentRound, _dbConnection);

        //Change the round in the model
        _model.CurrentRound = round;
        //And the session proxy
        _sessionProxy.CurrentRound = round;
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
            await ScoreKeeper.Save(_model.Tournament.Id, formScores, currentRound, _dbConnection);

            // Reload the scores for the new round
            await LoadScore();
        }
        catch (Exception ex)
        {
            _log.LogCritical(ex.Message);
        }

        return View("Index", _model);

    }

    public async Task<IActionResult> Print(int round, bool showScores)
    {
        //Get the current tournament from the gateway

        var tournament  = await _tourGateway.GetCurrentTournament();
        int tourId= _model.Tournament.Id;
        int currentRound = _model.CurrentRound;

        //call the builderSchedulefromdb method to get the schedule from the database
        //and build the schedule object
        //Get tournament details 
        var schedule = await BuildScheduleFromDB();

        List<Score>? formScores = null;
        //Get scores for the current round
        //from the form values
        if (showScores)
        {

            // FormReaderScoring formReader = new FormReaderScoring();
            // formScores = formReader.Parse(this.Request.Form, _sessionProxy?.TournamentId ?? 0);
            //Get the scores from the database
            formScores = await ProxyScores.GetScores(tourId, currentRound, _dbConnection);
        }

        _model.Tournament = tournament;
        _model.Schedule = schedule;
        _model.CurrentRound = currentRound;
        _model.Tournament.Schedule = schedule;

        //Load Page and return if the schedule is null
        if (schedule is null || _tournament is null)
        {
            await LoadPage();
            return Page();
        }


        //Use the PdfPrinter class to generate the PDF

        PdfPrinter pdfPrinter = new PdfPrinter(_schedule);
        //Get the response output stream
        //Send the PDF in the response.
        this.Response.ContentType = "application/pdf";

        //Call the print method to generate the PDF
        try
        {
            await pdfPrinter.Print(this.HttpContext.Response.BodyWriter.AsStream(true), tournament, schedule, currentRound,
            formScores);
        }
        catch(Exception ex)
        {
            _log.LogError(ex.Message);
        }   

        return Page();

    }

    public async Task<IActionResult> PrintScores(int round)
    {
    }
    {
    }

    public async Task<IActionResult> Reload()
    {
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
        _model.Tournament.Schedule = schedule;
        _model.Schedule = schedule;

        //Get scores using  ProxyScores method GetScores
        List<Score> listOfScores = await ProxyScores.GetScores(_model.Tournament.Id, _model.CurrentRound, _dbConnection);
        //use LINQ t6o filter scores by round order by permutation and set
        _model.RoundScores = listOfScores.Where(s => s.Round == _model.CurrentRound).OrderBy(s => s.Permutation).ThenBy(s => s.Match).ThenBy(s => s.Set).ToList();



    }


    //Build schedule from the database
    private async Task<Schedule?> BuildScheduleFromDB()
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

        BuilderSchedule builderSchedule = new BuilderSchedule(recordsSched, players, teams, _model.Tournament, _dbConnection);
        return builderSchedule.Create();

    }
}