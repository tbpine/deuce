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

    public string Title { get; set; } = "";

    private Schedule? _schedule;
    private Tournament? _tournament;

    private int _currentRound = 0;

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => _tournament?.Format?.NoSets ?? 1; }
    public int CurrentRound { get => _currentRound; }

    public Round Rounds(int r) => _schedule?.GetRounds(r) ?? new Round(0);


    public ScoringPageModel(ILogger<ScoringPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway, SessionProxy sessionProxy)
    : base(handlerNavItems, sp, config, tgateway,sessionProxy)
    {
        _log = log;
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
        _tournament =  await _tourGatway?.GetCurrentTournament()!;
        if (_tournament is  null) return;

        IGameMaker gm = new GameMakerTennis();

        //Get teams for this tournament
        using var scope = _serviceProvider.CreateScope();
        await using var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        
        if (dbconn is  null)  return;
        
        dbconn.ConnectionString = _config.GetConnectionString("deuce_local");
        await dbconn.OpenAsync();
        DbRepoRecordTeamPlayer dbRepoTeamPlayer = new(dbconn);
        var recordsTeamPlayers = await dbRepoTeamPlayer.GetList();
        //Extract teams and players
        TeamRepo teamRepo = new TeamRepo(recordsTeamPlayers);
        List<Team> listOfTeams = teamRepo.ExtractFromRecordTeamPlayer();

        //Action
        //Assert
        FactorySchedulers fac = new();
        IScheduler matchMaker = fac.Create(_tournament, gm);
        _schedule = matchMaker.Run(listOfTeams);

    }


}