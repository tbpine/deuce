namespace deuce;

/// <summary>
/// A person involved in a match.
/// </summary>
public class Player
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------
    private List<Match> _history = new();
    private int _id;
    private string? _first;
    private string? _last;
    private string? _middle;
    private double _ranking;
    private int _index;
    private int _teamPlayerId;
    private Organization? _club;
    private Team? _team;
    private Tournament? _tournament;

    public int Id { get { return _id; } set { _id = value; } }
    public string? First { get { return _first; } set { _first = value; } }
    public string? Last { get { return _last; } set { _last = value; } }
    public string? Middle { get { return _middle; } set { _middle = value; } }
    public double Ranking { get { return _ranking; } set { _ranking = value; } }
    public int Index { get { return _index; } set { _index = value; } }
    public int TeamPlayerId { get { return _teamPlayerId; } set { _teamPlayerId = value; } }
    public Organization? Club { get { return _club; } set { _club = value; }}
    public Team? Team { get { return _team; } set { _team = value; }}
    
    /// <summary>
    /// Usually a Tournament DTO (Data transfer object)
    /// containing only the ID
    /// </summary>
    public Tournament? Tournament { get=>_tournament;set=>_tournament = value; }    

    public IEnumerable<Match> History { get => _history; }
    /// <summary>
    /// The empty constructor
    /// </summary>
    public Player()
    {

    }

    public void AddGame(Match game) => _history.Add(game);

    public override string ToString()
    {
        return _first + " " + _last;
    }

}