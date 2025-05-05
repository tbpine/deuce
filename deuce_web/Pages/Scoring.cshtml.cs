using Microsoft.AspNetCore.Mvc;
using deuce;
using System.Diagnostics;
using System.Data.Common;

/// <summary>
/// Add scores to a tournament.
/// </summary>
public class ScoringPageModel : BasePageModelAcc
{
    private readonly ILogger<ScoringPageModel> _log;
    private readonly DbRepoRecordTeamPlayer _deRepoRecordTeamPlayer;
    private readonly DbRepoRecordSchedule _dbRepoRecordSchedule;
    public string Title { get; set; } = "";

    private Schedule? _schedule;
    private Tournament? _tournament;

    private int _currentRound = 0;

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => _tournament?.Format?.NoSets ?? 1; }
    public int CurrentRound { get => _currentRound; }

    public Round Rounds(int r) => _schedule?.GetRounds(r) ?? new Round(0);

    private readonly DbConnection _dbConnection;
    private List<Score>? _roundScores;
    public List<Score>? RoundScores { get => _roundScores; }

    public ScoringPageModel(ILogger<ScoringPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbConnection dbConnection,
    DbRepoRecordSchedule dbRepoRecordSchedule)
    : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _deRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbConnection = dbConnection;
        _dbRepoRecordSchedule = dbRepoRecordSchedule;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            //Check if there's a query parameter in the URL called "action"
            var action = this.Request.Query["action"].ToString();
            //Get the round parameter from the URL
            var round = this.Request.Query["round"].ToString();
            int iRound = int.TryParse(round, out iRound) ? iRound : -1;
            
            if (!string.IsNullOrEmpty(action) && action == "print_scores" && iRound >= 0)
            {
                //If the action is "print_scores", call PrintSchedule with true
                return await PrintSchedule(true, int.Parse(round));
            }
            else if (!string.IsNullOrEmpty(action) && action == "print" && iRound >= 0)
            {
                return await PrintSchedule(false, int.Parse(round));
            }


            //Set the current round to zero in the session proxy
            _sessionProxy.CurrentRound = 0;

            await LoadPage();
            Title = _tournament?.Label ?? "";
        }
        catch (Exception ex)
        {
            _log.LogCritical(ex.Message);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");


        try
        {
            //Get round permutation and games
            int currentRound = _sessionProxy?.CurrentRound ?? 0;
            FormReaderScoring formReader = new FormReaderScoring();
            var formScores = formReader.Parse(this.Request.Form, _sessionProxy?.TournamentId ?? 0);
            //Use proxy to save
            await ScoreKeeper.Save(_sessionProxy?.TournamentId ?? 0, formScores, currentRound, _dbConnection);

            //Get the selected round from the form (what was selected on screen)  
            string? strCR = this.Request.Form["current_round"];
            _currentRound = int.Parse(strCR ?? "0");

            Title = _tournament?.Label ?? "";
            //Set session current round to the one selected on the form
            await LoadPage();
            if (_sessionProxy is not null) _sessionProxy.CurrentRound = _currentRound;
            return Page();

        }
        catch (Exception ex)
        {
            _log.LogCritical(ex.Message);
        }

        return Page();
    }

    private async Task LoadPage()
    {
        //Get the current tournament
        //DB access
        _tournament = await _tourGateway?.GetCurrentTournament()!;
        if (_tournament is null) return;

        IGameMaker gm = new GameMakerTennis();

        //Extract teams and players
        TeamRepo teamRepo = new TeamRepo(_tournament, _dbConnection);
        List<Team> listOfTeams = (await teamRepo.GetListAsync(_sessionProxy?.TournamentId ?? 0));

        //TODO: This is incorrect.
        //The schedule should be built from
        //rows of the match and match_player tables
        //respectively.
        //Get the schedule from the database
        _schedule = await BuildScheduleFromDB();

        //Get scores using  ProxyScores method GetScores
        List<Score> listOfScores = await ProxyScores.GetScores(_tournament.Id, _currentRound, _dbConnection);
        //use LINQ t6o filter scores by round order by permutation and set
        _roundScores = listOfScores.Where(s => s.Round == _currentRound).OrderBy(s => s.Permutation).ThenBy(s => s.Match).ThenBy(s => s.Set).ToList();



    }

    //Get the score given the round and permutation and match
    public List<Score>? GetScore(int round, int permutation, int match)
    {
        if (_roundScores is null) return null;
        return _roundScores.FindAll(s => s.Round == round && s.Permutation == permutation && s.Match == match);
    }

    //Build schedule from the database
    public async Task<Schedule?> BuildScheduleFromDB()
    {
        if (_tournament is null) return null;
        //Load schedule from the database

        List<RecordSchedule> recordsSched = await _dbRepoRecordSchedule.GetList(new Filter() { TournamentId = _tournament?.Id ?? 0 });

        //List of players and teams for this tournament
        var dbrepotp = new DbRepoRecordTeamPlayer(_dbConnection);
        List<RecordTeamPlayer> teamplayers = await dbrepotp.GetList(new Filter() { TournamentId = _tournament?.Id ?? 0 });

        PlayerRepo playerRepo = new PlayerRepo();
        TeamRepo teamRepo = new TeamRepo(teamplayers);

        List<Player> players = playerRepo.ExtractFromRecordTeamPlayer(teamplayers);
        List<Team> teams = teamRepo.ExtractFromRecordTeamPlayer();
        if (_tournament is not null) _tournament.Teams = teams;

        BuilderSchedule builderSchedule = new BuilderSchedule(recordsSched, players, teams, _tournament ?? new(), _dbConnection);
        return builderSchedule.Create();

    }


    private async Task<IActionResult> PrintSchedule(bool showScores , int round)
    {
        //Get the current tournament, 
        //DB access
        //Get the current round from the session proxy
        _currentRound = round;
        //Get the current tournament from the gateway
        _tournament = await _tourGateway.GetCurrentTournament();
        //call the builderSchedulefromdb method to get the schedule from the database
        //and build the schedule object
        //Get tournament details 
        _schedule = await BuildScheduleFromDB();
        List<Score>? formScores = null;
        //Get scores for the current round
        //from the form values
        if (showScores)
        {

            // FormReaderScoring formReader = new FormReaderScoring();
            // formScores = formReader.Parse(this.Request.Form, _sessionProxy?.TournamentId ?? 0);
            //Get the scores from the database
            formScores = await ProxyScores.GetScores(_tournament?.Id ?? 0, _currentRound, _dbConnection);
        }

        //Load Page and return if the schedule is null
        if (_schedule is null || _tournament is null)
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
            await pdfPrinter.Print(this.HttpContext.Response.BodyWriter.AsStream(true), _tournament, _schedule, _currentRound,
            formScores);
        }
        catch(Exception ex)
        {
            _log.LogError(ex.Message);
        }   

        return Page();

    }
}