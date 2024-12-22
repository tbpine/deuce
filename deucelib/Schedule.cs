namespace deuce;

/// <summary>
/// Defines when, where and who should play matches.
/// </summary>
public class Schedule
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private Dictionary<int, List<Match>> _schedule = new();
    private Tournament Tournament { get; init; }


    //------------------------------------
    //| Props                            |
    //------------------------------------

    public int NoRounds { get => _schedule.Keys.Count; }
    public List<Match>? GetMatches(int round) => _schedule[round];

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="t">Tournament reference</param>
    public Schedule(Tournament t)
    {
        Tournament = t;
    }

    /// <summary>
    /// Add a macth
    /// </summary>
    /// <param name="round">Which round </param>
    /// <param name="home">Player 1</param>
    /// <param name="away">Player 2</param>
    public void AddMatch(int round, Player home, Player away)
    {
        //A a collection of matches for the round
        List<Match> matches = _schedule.ContainsKey(round) ? _schedule[round] :
                new List<Match>();

        if (!_schedule.ContainsKey(round)) _schedule.Add(round, matches);

        //Make the games
        Match game = new Match("", round, new Player[] { home!, away! });
        matches.Add(game);

    }

    /// <summary>
    /// Add a range of matches
    /// </summary>
    /// <param name="round">Round</param>
    /// <param name="toAdd">List of matches</param>
    public void AddRange(int round, IEnumerable<Match> toAdd)
    {
        //A a collection of matches for the round
        List<Match> matches = _schedule.ContainsKey(round) ? _schedule[round] :
                new List<Match>();

        if (!_schedule.ContainsKey(round)) _schedule.Add(round, matches);

        matches.AddRange(toAdd);

    }
}