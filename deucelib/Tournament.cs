namespace deuce;

public class Tournament
{
    private int _id;
    private string _label = "";
    private DateTime _start = DateTime.Now;
    private DateTime _end;
    private int _steps;
    private int _type;
    private int _max;
    // private int _TeamSize;
    private int _sport;
    private int _entryType;
    private TournamentDetail _detail = new();
    private double _fee;
    private bool _useRankings;
    private double _prize;

    TournamentStatus _status;

    private List<Team> _teams = new();

    private Schedule? _schedule;

    private int _interval;
    private Organization _organization = new();

    public int Id { get { return _id; } set { _id = value; } }

    public string Label { get { return _label; } set { _label = value; } }

    [Display("Start Date", "dd/MM/yyyy")]
    public DateTime Start { get { return _start; } set { _start = value; } }
    
    [Display("Finish Date", "dd/MM/yyyy")]
    public DateTime End { get { return _end; } set { _end = value; } }

    [Display("Repeats every", null, typeof(Interval) )]
    public int Interval { get { return _interval; } set { _interval = value; } }
    // public int TeamSize { get { return _TeamSize; } set { _TeamSize = value; } }
    public int Steps { get { return _steps; } set { _steps = value; } }
    public int Sport { get { return _sport; } set { _sport = value; } }
    public int Max { get { return _max; } set { _max = value; } }
    public TournamentDetail Details { get { return _detail; } set { _detail = value; } }
    public int Type { get { return _type; } set { _type = value; } }
    public int EntryType { get { return _entryType; } set { _entryType = value; } }

    [Display("Entry Fee", "c2")]
    public double Fee { get { return _fee; } set { _fee = value; } }

    [Display("Prize", "c2")]
    public double Prize { get { return _prize; } set { _prize = value; } }
    public bool UseRanking { get { return _useRankings; } set { _useRankings = value; } }

    public Schedule? Schedule { get => _schedule; set => _schedule = value; }

    public List<Team> Teams { get => _teams; set => _teams = value; }
    public Organization Organization { get => _organization; set => _organization = value; }

    /// <summary>
    /// A flag indicating the state of the tournament
    /// </summary>
    public TournamentStatus Status { get=>_status ; set=>_status = value; }
}