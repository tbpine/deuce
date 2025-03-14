namespace deuce;

/// <summary>
/// A member of this site
/// </summary>
public class Member
{
    //------------------------------------
    //| Internals                        |
    //------------------------------------
    private int _id;
    private string? _first;
    private string? _last;
    private string? _middle;
    private double _utr;
    private int _countryCode;

    public int Id { get { return _id; } set { _id = value; } }
    public string? First { get { return _first; } set { _first = value; } }
    public string? Last { get { return _last; } set { _last = value; } }
    public string? Middle { get { return _middle; } set { _middle = value; } }
    public double Utr { get { return _utr; } set { _utr = value; } }
    public int CountryCode { get { return _countryCode; } set { _countryCode = value; } }
    
    /// <summary>
    /// The empty constructor
    /// </summary>
    public Member()
    {

    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(_middle))
            return  _first + " "+ _middle + " " + _last;
        else
            return _first + " " + _last;
    }

}