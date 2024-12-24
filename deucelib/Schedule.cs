namespace deuce;

/// <summary>
/// Defines when, where and who should play matches.
/// </summary>
public class Schedule
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private Dictionary<int, List<Round>> _schedule = new();
    private Tournament Tournament { get; init; }


    //------------------------------------
    //| Props                            |
    //------------------------------------

    public int NoRounds { get => _schedule.Keys.Count; }
    public List<Round>? GetRounds(int round) => _schedule[round];

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
    /// <param name="roundNo">Round to add</param>
    public void AddRound(Round round, int roundNo)
    {
        //A a collection of matches for the round
        List<Round> rounds = _schedule.ContainsKey(roundNo) ? _schedule[roundNo] :
                new List<Round>();

        if (!_schedule.ContainsKey(roundNo)) _schedule.Add(roundNo, rounds);

        //Make the games
        rounds.Add(round);

    }

    /// <summary>
    /// Add a range of matches
    /// </summary>
    /// <param name="roundNo">Round</param>
    /// <param name="toAdd">List of matches</param>
    public void AddRange(int roundNo, IEnumerable<Round> toAdd)
    {
        //A a collection of matches for the round
        List<Round> rounds = _schedule.ContainsKey(roundNo) ? _schedule[roundNo] :
                new List<Round>();

        if (!_schedule.ContainsKey(roundNo)) _schedule.Add(roundNo, rounds);

        rounds.AddRange(toAdd);

    }
}