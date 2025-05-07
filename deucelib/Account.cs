namespace deuce;

/// <summary>
/// Account class to hold the user details.
/// </summary>
public class Account
{
    private string _password = "";
    private string _email = "";
    private int _id;
    private int _type;
    private bool _active = true;
    private int _player;
    private int _organization;
    private int _country;

    public int Id { get { return _id; } set { _id = value; } }

    public string Email { get { return _email; } set { _email = value; } }

    public string Password { get { return _password; } set { _password = value; } }

    public int Type { get { return _type; } set { _type = value; } }

    public bool Active { get { return _active; } set { _active = value; } }
    public int Player { get { return _player; } set { _player = value; } }
    public int Organization { get { return _organization; } set { _organization = value; } }
    public int Country { get { return _country; } set { _country = value; } }
   
 
    
}