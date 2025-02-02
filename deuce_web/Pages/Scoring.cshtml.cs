using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using deuce;
using System.Diagnostics;
using System.Data.Common;

/// <summary>
/// 
/// </summary>
public class ScoringPageModel : AccBasePageModel
{
    private readonly ILogger<ScoringPageModel> _log;

    public string Title { get; set; } = "";

    private Schedule? _schedule;
    private Tournament? _t;

    private int _currentRound = 0;

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => _t?.Format?.NoSets ?? 1; }
    public int CurrentRound { get => _currentRound; }

    public Round Rounds(int r) => _schedule?.GetRounds(r) ?? new Round(0);


    public ScoringPageModel(ILogger<ScoringPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config,
    ITournamentGateway tgateway)
    : base(handlerNavItems, sp, config, tgateway)
    {
        _log = log;
    }

    public void OnGet()
    {
        LoadCurrentTournament();
        Title = _t?.Label ?? "";
    }

    public void OnPost()
    {
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");
        string? strCR = this.Request.Form["current_round"];
        _currentRound = int.Parse(strCR ?? "0");
        LoadCurrentTournament();
        Title = _t?.Label ?? "";
    }

    private async Task LoadCurrentTournament()
    {
        //Get the current tournament
        //DB access
        var tournament = await _tourGatway?.GetCurrentTournament();
        if (tournament is  null) return;

        IGameMaker gm = new GameMakerTennis();

        //Get teams for this tournament
         var scope = _serviceProvider.CreateScope();
        var dbconn = scope.ServiceProvider.GetRequiredService<DbConnection>();
        
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
        var matchMaker = fac.Create(tournament, gm);
        _schedule = matchMaker.Run(listOfTeams);

    }


}