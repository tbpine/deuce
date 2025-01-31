using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using deuce;
using System.Diagnostics;

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


    public ScoringPageModel(ILogger<ScoringPageModel> log, ISideMenuHandler handlerNavItems, IServiceProvider sp, IConfiguration config)
    :base(handlerNavItems, sp,  config)
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
        _currentRound = int.Parse(strCR??"0");
        LoadCurrentTournament();
        Title = _t?.Label ?? "";
    }

    private void LoadCurrentTournament()
    {

        //Assign
        _t = new Tournament();
        _t.Type = 1;
        //1 for tennis for now.
        _t.Sport = 1;
        _t.Format = new Format(2, 2, 1);
        _t.TeamSize = 2;
        _t.Start = DateTime.Now;
        _t.End = DateTime.Now;
        _t.Interval = 1;
        _t.Label = "Wicks Open";

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();
        Team? currentTeam = null;

        for (int i = 0; i < 8; i++)
        {
            if (i % _t.TeamSize == 0)
            {
                currentTeam = new Team(i + 1, $"team_{i + 1}");
                teams.Add(currentTeam);
            }

            currentTeam?.AddPlayer(new Player
            {
                Id = i + 1,
                First = $"player_{i}",
                Last = $""
            });



        }

        //Action
        //Assert
        FactorySchedulers fac = new();
        var mm = fac.Create(_t, gm);
        _schedule = mm.Run(teams);

    }

   
}