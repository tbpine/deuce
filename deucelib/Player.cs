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
    private double _ranking;
    private string? _last;
    private int _index;
    private Organization? _club;

    public int Id { get { return _id; } set { _id = value; } }
    public string? First { get { return _first; } set { _first = value; } }
    public string? Last { get { return _last; } set { _last = value; } }
    public double Ranking { get { return _ranking; } set { _ranking = value; } }
    public int Index { get { return _index; } set { _index = value; } }
    public Organization? Club { get { return _club; } set { _club = value; }}
    
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