namespace deuce;

/// <summary>
/// Defines the match parameters
/// </summary>
public class Format
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    /// <summary>
    /// Tennis specific : the type of a matches played
    /// </summary>

    private int _noDoubles = 0;
    private int _noSingles = 1;
    private int _noSets = 1;

    public int NoSingles { get { return _noSingles; } set { _noSingles = value; } }
    public int NoDoubles { get { return _noDoubles; } set { _noDoubles = value; } }
    public int NoSets { get { return _noSets; } set { _noSets = value; } }

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="noSingle"></param>
    /// <param name="noDoubles"></param>
    /// <param name="noSets"></param>
    public Format(int noSingle, int noDoubles, int noSets)
    {
        _noSingles = noSingle;
        _noDoubles = noDoubles;
        _noSets = noSets;
    }


}