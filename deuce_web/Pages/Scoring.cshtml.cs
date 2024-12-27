using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using deuce;
using System.Diagnostics;

/// <summary>
/// 
/// </summary>
public class ScoringPageModel : PageModel
{
    private readonly ILogger<ScoringPageModel> _log;

    public string Title { get; set; } = "";

    private Schedule? _schedule;
    private Tournament? _t;

    private int _currentRound = 0;

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => _t?.Format?.NoSets ?? 1; }
    public int CurrentRound { get => _currentRound; }

    public IEnumerable<Round> Rounds(int r) => _schedule?.GetRounds(r) ?? new List<Round>();


    public ScoringPageModel(ILogger<ScoringPageModel> log)
    {
        _log = log;
    }

    public void OnGet()
    {
        MakeDummyTournament();
        Title = _t?.Label ?? "";
    }

    public void OnPost()
    {
        foreach (var kp in this.Request.Form)
            Debug.Write(kp.Key + "=" + kp.Value + "\n");
        string? strCR = this.Request.Form["current_round"];
        _currentRound = int.Parse(strCR??"0");
        MakeDummyTournament();
        Title = _t?.Label ?? "";
    }

    private void MakeDummyTournament()
    {
        //Assign
        _t = new Tournament();
        _t.Type = new TournamentType(1, "Round Robbin");
        //1 for tennis for now.
        _t.Sport = 1;
        _t.Format = new Format(2, 2, 1);
        _t.TeamSize = 2;
        _t.Start = DateTime.Now;
        _t.End = DateTime.Now;
        _t.Interval = new Interval(1, "Weekly");
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

    public string GetContent(int roundIdx, int subIndex, int match, int col)
    {
        var list = _schedule?.GetRoundAtIndex(roundIdx);
        if (list is null) return "";

        Round round = list[subIndex];
        Match m = round.GetMatchAtIndex(match);

        if (m is null) return "";

        string gameType = m.IsDouble ? "Doubles" : "Singles";
        string teamHome = m.IsDouble ? m.GetPlayerAt(0).ToString() + "/" + m.GetPlayerAt(1).ToString() : m.GetPlayerAt(0).ToString();
        string teamAway = m.IsDouble ? m.GetPlayerAt(2).ToString() + "/" + m.GetPlayerAt(3).ToString() : m.GetPlayerAt(1).ToString();
        int noSets = _t?.Format?.NoSets ?? 1;

        int noColumns = noSets + 1;
        int rowIdx = col / noColumns;
        int colIdx = col % noColumns;

        if (rowIdx == 0 && col == 0) return gameType;
        else if (rowIdx == 0 && colIdx < noSets) return $"Set {col}";
        else if (rowIdx == 1 && colIdx == 0) return teamHome;
        else if (rowIdx == 1 && colIdx < noSets) return "<input class='border w-25'></input>";
        else if (rowIdx == 2 && colIdx == 0) return teamAway;
        else if (rowIdx == 2 && colIdx < noSets) return "<input class='border w-25'></input>";

        return "";


    }
}