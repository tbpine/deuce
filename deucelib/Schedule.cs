namespace deuce;

/// <summary>
/// Defines when, where and who should play matches.
/// </summary>
public class Schedule
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private List<Round> _rounds = new();
    private Tournament Tournament { get; init; }


    //------------------------------------
    //| Props                            |
    //------------------------------------

    public int NoRounds { get => _rounds.Count; }
    public Round GetRounds(int round) => _rounds[round];

    public Round GetRoundAtIndex(int index) => _rounds[index];

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
    /// <param name="permutation">Perm to add</param>
    /// <param name="roundNo">Round number</param>
    public void AddPermutation(Permutation permutation, int roundNo, string roundLabel ="")
    {
        var existing = _rounds.Find(e=>e.Index == roundNo);
        Round round = existing is null ? new Round(roundNo) : existing;
        //Label if specified
        round.Label = string.IsNullOrEmpty(roundLabel) ?  round.Label: roundLabel;
        if (existing is null)
        {
            round.Tournament = Tournament;
            _rounds.Add(round);

        } 
        //Soft Link
        permutation.Round=round;
        round.AddPerm(permutation);

    }

    /// <summary>
    /// Add a range of matches
    /// </summary>
    /// <param name="roundNo">Round</param>
    /// <param name="toAdd">List of permutations</param>
    public void AddRange(int roundNo, IEnumerable<Permutation> toAdd)
    {
        //A a collection of matches for the round
       var existing = _rounds.Find(e=>e.Index == roundNo);
        Round round = existing is null ? new Round(roundNo) : existing;
        
        if (existing is null) 
        {
            round.Tournament = Tournament;
            _rounds.Add(round);
        }
        
        foreach(var perm in toAdd) perm.Round = round;
        existing?.AddRange(toAdd);

    }
}