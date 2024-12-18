namespace deuce.lib;

public class Tournament
{
    private int _id;
    private string? _label;
    private DateTime _start;
    private DateTime _end;
    private int _steps;
    private TournamentType? _type;
    private int _max;
    private double _fee;
    private bool _useRankings;
    private double _prize;
    private Interval? _interval;
    public int Id { get { return _id; } set { _id = value; } }
    public string? Label { get { return _label; } set { _label = value; } }
    public DateTime Start { get { return _start; } set { _start = value; } }
    public DateTime End { get { return _end; } set { _end = value; } }
    public Interval? Interval { get { return _interval; } set { _interval = value; } }
    public int Steps { get { return _steps; } set { _steps = value; } }
    public TournamentType? Type { get { return _type; } set { _type = value; } }
    public int Max { get { return _max; } set { _max = value; } }
    public double Fee { get { return _fee; } set { _fee = value; } }
    public double Prize { get { return _prize; } set { _prize = value; } }
    public bool UseRankings { get { return _useRankings; } set { _useRankings = value; } }

}