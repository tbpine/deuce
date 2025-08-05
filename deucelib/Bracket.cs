namespace deuce;

public class Bracket
{
    // internal members
    private int _id;
    private int _upperId;
    private int _tournamentId;

    /// <summary>
    /// Link to the upper bracket.
    /// This is used to link the bracket to the parent tournament/
    /// </summary>
    private Tournament? _upper;

    /// <summary>
    /// The mini tournament
    /// </summary>
    private Tournament? _tournament;

    public int Id { get { return _id; } set { _id = value; } }
    public int UpperId { get { return _upperId; } set { _upperId = value; } }
    public int TournamentId { get { return _tournamentId; } set { _tournamentId = value; } }

    public Tournament? Upper { get { return _upper; } set { _upper = value; } }
    public Tournament? Tournament { get { return _tournament; } set { _tournament = value; } }

    /// <summary>
    /// Construct a bracket with an id, upper bracket id and tournament id.
    /// </summary>
    /// <param name="id"> Unique integer identifier for this bracket </param>
    /// <param name="upper"> Upper bracket id</param>
    /// <param name="tournament"> Tournament id</param>
    public Bracket(int id, int upper, int tournament)
    {
        _id = id;
        _upperId = upper;
        _tournamentId = tournament;
    }

    /// <summary>
    /// The empty constructor    
    /// </summary>
    public Bracket()
    {
    }


};
