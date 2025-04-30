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

    public string Title { get; set; } = "";

    private Schedule? _schedule;
    private Tournament? _tournament;

    private int _currentRound = 0;

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => _tournament?.Format?.NoSets ?? 1; }
    public int CurrentRound { get => _currentRound; }

    public Round Rounds(int r) => _schedule?.GetRounds(r) ?? new Round(0);

    private readonly DbConnection _dbConnection;

    public ScoringPageModel(ILogger<ScoringPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy, DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer, DbConnection dbConnection)
    : base(handlerNavItems, sp, config, tgateway, sessionProxy)
    {
        _log = log;
        _deRepoRecordTeamPlayer = dbRepoRecordTeamPlayer;
        _dbConnection = dbConnection;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadCurrentTournament();
        Title = _tournament?.Label ?? "";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");
            
        await LoadCurrentTournament();

        if (this.Request.Form["action"] == "save")
        {
            //Get round permutation and games

            FormReaderScoring formReader = new FormReaderScoring();
            List<Score> formScores = formReader.Parse(this.Request.Form, _schedule??new(_tournament??new()), _currentRound, _tournament?.Id ?? 0);
            //Use proxy to save
            ProxyScores.Save(formScores, _dbConnection);

        }

        string? strCR = this.Request.Form["current_round"];
        _currentRound = int.Parse(strCR ?? "0");
        await LoadCurrentTournament();
        Title = _tournament?.Label ?? "";

        return Page();
    }

    private async Task LoadCurrentTournament()
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

        FactorySchedulers fac = new();
        IScheduler matchMaker = fac.Create(_tournament, gm);
        _schedule = matchMaker.Run(listOfTeams);
        
    }



}