namespace deuce;

/// <summary>
/// Defines when, where and who should play matches.
/// </summary>
public class Draw
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private List<Round> _rounds = new();
    private Tournament Tournament { get; init; }
    //Keep a soft link to the group if any
    private Group? _group;

    //------------------------------------
    //| Props                            |
    //------------------------------------

    public int NoRounds { get => _rounds.Count; }
    public Round GetRound(int idx) => _rounds[idx];

    public IEnumerable<Round> Rounds => _rounds;

    public Group? Group { get => _group; set => _group = value; }

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="t">Tournament reference</param>
    public Draw(Tournament t)
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