namespace deuce;

/// <summary>
/// Defines the match parameters
/// </summary>
public class Format
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------

    private int _noDoubles;
    private int _noSingles;
    public int NoSingles { get { return _noSingles; } set { _noSingles = value; } }

    public int NoDoubles { get { return _noDoubles; } set { _noDoubles = value; } }

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="noSingle"></param>
    /// <param name="noDoubles"></param>
    public Format(int noSingle, int noDoubles)
    {
        _noSingles = noSingle;
        _noDoubles = noDoubles;
    }


}