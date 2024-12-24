namespace deuce;

/// <summary>
/// Team vs Team
/// </summary>
public class Round
{
    //The list of competing teams
    private List<Team> _teams = new();
    private List<Match> _matches = new();

    private readonly int _number;
    public int NoRounds { get => _number; }
    public int NoMatches { get => _matches.Count; }
    public int NoTeams { get => _teams.Count; }

    public IEnumerable<Team> Teams { get => _teams; }
    public IEnumerable<Match> Matches { get => _matches; }


    /// <summary>
    /// Construct with values
    /// </summary>
    public Round(int number, params Team[] teams)
    {
        _number = number;
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