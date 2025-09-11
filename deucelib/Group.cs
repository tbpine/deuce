using DocumentFormat.OpenXml.Office2021.Excel.RichDataWebImage;

namespace deuce;

/// <summary>
/// Define a group. Has it's own draw and teams.
/// </summary>
public class Group
{
    private Draw? _draw;
    private List<Team> _teams = new();

    private int _id;
    private string _label = "";

    public int Id { get { return _id; } set { _id = value; } }
    public string Label { get { return _label; } set { _label = value; } }

    public Draw? Draw { get { return _draw; } set { _draw = value; } }
    public IEnumerable<Team> Teams { get { return _teams; } }

    /// <summary>
    /// Construct an empty group
    /// </summary>
    public Group()
    {
    }


    public void CreateDraw(IDrawMaker drawMaker)
    {
        //Validate parameters
        if (drawMaker == null) throw new ArgumentNullException(nameof(drawMaker));
        if (_teams == null) throw new ArgumentNullException(nameof(_teams));
        if (_teams.Count < 2) throw new ArgumentException("At least two teams are required to create a draw.");

        _draw = drawMaker.Create(_teams);
    }

    public void ProgressDraw(IDrawMaker drawMaker, int round, int previousRound, List<Score> scores)
    {
        //Validate parameters
        if (drawMaker == null) throw new ArgumentNullException(nameof(drawMaker));
        if (_draw == null) throw new InvalidOperationException("Draw has not been created.");
        if (_teams == null) throw new ArgumentNullException(nameof(_teams));
        if (_teams.Count < 2) throw new ArgumentException("At least two teams are required to progress a draw.");

        drawMaker.OnChange(_draw, round, previousRound, scores);
    }
    
    public void AddTeam(Team team)
    {
        if (!_teams.Contains(team))
        {
            _teams.Add(team);
            team.Group = this;
        }
    }
}