namespace deuce;

/// <summary>
/// Team vs Team
/// </summary>
public class Permutation
{
    //The list of competing teams
    private List<Team> _teams = new();
    private List<Match> _matches = new();
    private Round? _round;
    private int _id;

    // private int _index;
    // public int RoundIndex { get => _index; }
    public int NoMatches { get => _matches.Count; }
    public int NoTeams { get => _teams.Count; }
    public Round? Round { get=>_round; set=>_round = value; }

    public IEnumerable<Team> Teams { get => _teams; }
    public IEnumerable<Match> Matches { get => _matches; }
    public int Id { get { return _id; } set { _id = value; }}

    /// <summary>
    /// Construct with values
    /// </summary>
    public Permutation(int number, params Team[] teams)
    {
        _id =number;
        _teams.AddRange(teams);
    }

    

    /// <summary>
    /// Get a team at index
    /// </summary>
    /// <param name="index">Index</param>
    /// <returns>Team at index</returns>
    public Team GetTeamAtIndex(int index) => _teams[index];
    public Match GetMatchAtIndex(int index) => _matches[index];
   public void AddMatch(Match match)=>_matches.Add(match);
   
}