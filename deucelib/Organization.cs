namespace deuce;
/// <summary>
/// 
/// </summary>
public class Organization
{
    private bool _active;
    private string _abn = "";
    private string _owner = "";
    private string _name = "";
    private int _id;
    public int Id { get { return _id; } set { _id = value; } }

    public string Name { get { return _name; } set { _name = value; } }

    public string Owner { get { return _owner; } set { _owner = value; } }

    public string Abn { get { return _abn; } set { _abn = value; } }

    public bool Active { get { return _active; } set { _active = value; } }

    /// <summary>
    /// Empty Constructor
    /// </summary>
    public Organization()
    {

    }


}