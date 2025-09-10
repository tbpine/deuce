namespace deuce;

/// <summary>
/// Define a group of teams
/// </summary>
public class Group
{
    private int _id;
    private string _label = "";

    public int Id { get { return _id; } set { _id = value; } }
    public string Label { get { return _label; } set { _label = value; } }

    /// <summary>
    /// Construct an empty group
    /// </summary>
    public Group()
    {
    }

    /// <summary>
    /// Construct a group with id and label
    /// </summary>
    /// <param name="id">Database identifier</param>
    /// <param name="label">Group name</param>
    public Group(int id, string label)
    {
        _id = id;
        _label = label;
    }
}