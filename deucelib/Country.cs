namespace deuce;

/// <summary>
/// Country where the tournament is held
/// </summary>
public class Country
{

    private int _code;

    private string _name;

    public int Code { get => _code; }

    public string Name { get => _name;  }

    /// <summary>
    /// Construct with values
    /// </summary>
    /// <param name="code">Country code</param>
    /// <param name="name">Name</param>
    public Country(int code, string name)
    {
        _code = code;
        _name = name;
    }

}