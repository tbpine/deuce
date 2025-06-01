using deuce;



public class ViewModelMember
{
    private Account _account = new();

    private ISideMenuHandler _sideMenuHandler;
    private Schedule? _schedule;

    private int _currentRound = 0;
    private List<Score>? _roundScores;

    public Account Account { get => _account; set => _account = value; }

    public IEnumerable<NavItem> NavItems { get => _sideMenuHandler.NavItems; }
    /// <summary>
    /// List of intervals (e.g., time intervals for scheduling).
    /// </summary>
    public List<Interval> Intervals { get; set; } = new();

    /// <summary>
    /// List of tournament types (e.g., single elimination, round-robin).
    /// </summary>
    public List<TournamentType> TournamentTypes { get; set; } = new();

    public Tournament Tournament { get; set; } = new();
    public TournamentDetail TournamentDetail { get; set; } = new();

    public Organization Organization { get; set; } = new();

    public int NoRounds { get => _schedule?.NoRounds ?? 0; }
    public int NoSets { get => Tournament?.Details?.Sets ?? 1; }
    public int CurrentRound { get => _currentRound; set => _currentRound = value; }

    public Schedule? Schedule { get => _schedule; set => _schedule = value; }

    public string Error { get; set; } = "";

    //For summary
    public string HtmlTour { get; set; } = "";
    public string HtmlTourDetail { get; set; } = "";
    public string Title { get; set; } = "";
    
    /// <summary>
    /// List of tournaments.
    /// </summary>
    public List<Tournament> Tournaments { get; set; } = new();

    public ViewModelMember(ISideMenuHandler sidemenu)
    {
        _sideMenuHandler = sidemenu;
    }

    public List<Score>? RoundScores { get => _roundScores; set => _roundScores = value; }

    public Round Rounds(int r) => _schedule?.GetRounds(r) ?? new Round(0);
    
      //Get the score given the round and permutation and match  //Get the score given the round and permutation and match
    public List<Score>? GetScore(int round, int permutation, int match)
    {
        if (_roundScores is null) return null;
        return _roundScores.FindAll(s => s.Round == round && s.Permutation == permutation && s.Match == match);
    }


}