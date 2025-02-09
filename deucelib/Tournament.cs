namespace deuce;

public class Tournament
{
    private int _id;
    private string? _label;
    private DateTime _start;
    private DateTime _end;
    private int _steps;
    private int _type;
    private int _max;
    private int _TeamSize;
    private int _sport;
    private int _entryType;
    private Format? _format;
    private double _fee;
    private bool _useRankings;
    private double _prize;

    private List<Team>? _teams;

    private Schedule? _schedule;

    private int _interval;
    private Organization? _organization;

    public int Id { get { return _id; } set { _id = value; } }

    public string? Label { get { return _label; } set { _label = value; } }
    public DateTime Start { get { return _start; } set { _start = value; } }
    public DateTime End { get { return _end; } set { _end = value; } }
    public int Interval { get { return _interval; } set { _interval = value; } }
    public int TeamSize { get { return _TeamSize; } set { _TeamSize = value; } }
    public int Steps { get { return _steps; } set { _steps = value; } }
    public int Sport { get { return _sport; } set { _sport = value; } }
    public int Max { get { return _max; } set { _max = value; } }
    public Format? Format { get { return _format; } set { _format = value; } }
    public int Type { get { return _type; } set { _type = value; } }
    public int EntryType { get { return _entryType; } set { _entryType = value; } }
    public double Fee { get { return _fee; } set { _fee = value; } }
    public double Prize { get { return _prize; } set { _prize = value; } }
    public bool UseRanking { get { return _useRankings; } set { _useRankings = value; } }

    public Schedule? Schedule { get => _schedule; set => _schedule = value; }

    public List<Team>? Teams { get => _teams; set => _teams = value; }
    public Organization? Organization { get => _organization; set => _organization = value; }


}