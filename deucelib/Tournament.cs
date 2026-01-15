using System.Xml.Serialization;

namespace deuce;

public class Tournament : ICloneable
{
    private int _id;
    private string _label = "";
    private DateTime _start = DateTime.Now;
    private DateTime _end;
    private int _steps;
    private int _type;
    private int _max;
    // private int _TeamSize;
    private int _sport;
    private int _entryType;
    private TournamentDetail _detail = new();
    private double _fee;
    private bool _useRankings;
    private double _prize;

    TournamentStatus _status;

    private List<Team> _teams = new();
    private List<Group> _groups = new();
    private Dictionary<int, List<TeamStanding>> _standings = new();

    /// <summary>
    /// A list of brackets for the tournament.
    /// </summary>
    private List<Bracket> _brackets = new();

    private Draw? _draw;

    private int _interval;
    private Organization _organization = new();

    public int Id { get { return _id; } set { _id = value; } }

    public string Label { get { return _label; } set { _label = value; } }

    [Display("Start Date", "dd/MM/yyyy")]
    public DateTime Start { get { return _start; } set { _start = value; } }

    [Display("Finish Date", "dd/MM/yyyy")]
    public DateTime End { get { return _end; } set { _end = value; } }

    [Display("Repeats every", null, typeof(Interval))]
    public int Interval { get { return _interval; } set { _interval = value; } }
    // public int TeamSize { get { return _TeamSize; } set { _TeamSize = value; } }
    public int Steps { get { return _steps; } set { _steps = value; } }
    public int Sport { get { return _sport; } set { _sport = value; } }
    public int Max { get { return _max; } set { _max = value; } }
    public TournamentDetail Details { get { return _detail; } set { _detail = value; } }
    public int Type { get { return _type; } set { _type = value; } }
    public int EntryType { get { return _entryType; } set { _entryType = value; } }

    [Display("Entry Fee", "c2")]
    public double Fee { get { return _fee; } set { _fee = value; } }

    [Display("Prize", "c2")]
    public double Prize { get { return _prize; } set { _prize = value; } }
    public bool UseRanking { get { return _useRankings; } set { _useRankings = value; } }

    public Draw? Draw { get => _draw; set => _draw = value; }

    public List<Team> Teams { get => _teams; set => _teams = value; }
    public List<TeamStanding> Standings 
    { 
        get => GetCurrentStandings();
        set => throw new InvalidOperationException("Use SetStandingsForRound to update standings");
    }
    public Organization Organization { get => _organization; set => _organization = value; }

    public IEnumerable<Bracket> Brackets { get => _brackets; }
    public IEnumerable<Group> Groups { get => _groups; }



    /// <summary>
    /// A flag indicating the state of the tournament
    /// </summary>
    public TournamentStatus Status { get => _status; set => _status = value; }

    /// <summary>
    /// Add a bracket to the tournament.
    /// This is used to link the bracket to the tournament.
    /// </summary>
    /// <param name="bracket"> The bracket to add to the tournament.</param>
    public void AddBracket(Bracket bracket)
    {
        if (bracket != null)
        {
            _brackets.Add(bracket);
        }
    }

    /// <summary>
    /// Clear all brackets from the tournament.
    /// This is used to reset the tournament and remove all brackets.
    /// </summary>
    public void ClearBrackets()
    {
        _brackets.Clear();
    }

    public void AddGroup(Group group)
    {
        if (group != null)
        {
            _groups.Add(group);
        }
    }

    public void ClearGroups()
    {
        _groups.Clear();
    }

    /// <summary>
    /// Sets the standings for a specific round.
    /// </summary>
    /// <param name="roundNumber">The round number (0-based)</param>
    /// <param name="standings">The standings to set for this round</param>
    public void SetStandingsForRound(int roundNumber, List<TeamStanding> standings)
    {
        _standings[roundNumber] = standings?.Select(s => new TeamStanding
        {
            Team = s.Team,
            Wins = s.Wins,
            Losses = s.Losses,
            Draws = s.Draws,
            Points = s.Points,
            Position = s.Position
        }).ToList() ?? new List<TeamStanding>();
    }

    /// <summary>
    /// Gets the standings for a specific round.
    /// </summary>
    /// <param name="roundNumber">The round number (0-based)</param>
    /// <returns>List of team standings for the round, or null if round not found</returns>
    public List<TeamStanding>? GetStandingsForRound(int roundNumber)
    {
        return _standings.TryGetValue(roundNumber, out var standings) ? standings : null;
    }

    /// <summary>
    /// Gets all historical standings data.
    /// </summary>
    /// <returns>Dictionary with round numbers as keys and standings as values</returns>
    public Dictionary<int, List<TeamStanding>> GetAllStandings()
    {
        return new Dictionary<int, List<TeamStanding>>(_standings);
    }

    /// <summary>
    /// Gets the current standings (most recent round).
    /// </summary>
    /// <returns>Current standings or empty list if no rounds completed</returns>
    public List<TeamStanding> GetCurrentStandings()
    {
        if (!_standings.Any()) return new List<TeamStanding>();
        
        var latestRound = _standings.Keys.Max();
        return _standings[latestRound];
    }

    /// <summary>
    /// Updates the current tournament standings (for backwards compatibility).
    /// </summary>
    /// <param name="standings">The new standings to set</param>
    [Obsolete("Use SetStandingsForRound instead")]
    public void UpdateStandings(List<TeamStanding> standings)
    {
        if (!_standings.Any())
        {
            SetStandingsForRound(0, standings);
        }
        else
        {
            var latestRound = _standings.Keys.Max();
            SetStandingsForRound(latestRound, standings);
        }
    }

    /// <summary>
    /// Clears all current standings.
    /// </summary>
    public void ClearStandings()
    {
        _standings.Clear();
    }

    /// <summary>
    /// Creates a shallow copy of the tournament.
    /// This is used to create a new instance of the tournament with the same properties.
    /// </summary>
    /// <returns> A new instance of the tournament with the same properties.</returns>
    public object Clone()
    {
        //Shallow copy of the tournament
        Tournament clone = (Tournament)this.MemberwiseClone();
        //Clone the details
        clone._detail = (TournamentDetail)this.Details.Clone();
        return clone;
    }
    
    public void SortGroups()
    {
        _groups = _groups.OrderBy(g => g.Index).ToList();
    }

  
}