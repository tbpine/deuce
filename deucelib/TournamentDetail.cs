namespace deuce;

public class TournamentDetail
{
    private int _tournamentId;
    private int _noEntries;
    private int _sets;
    private int _games;
    private int _customGames;
    private int _noSingles;
    private int _noDoubles;
    private int _teamSize;

    public int TournamentId { get { return _tournamentId; } set { _tournamentId = value; } }
    public int NoEntries { get { return _noEntries; } set { _noEntries = value; } }
    public int Sets { get { return _sets; } set { _sets = value; } }
    public int Games { get { return _games; } set { _games = value; } }
    public int CustomGames { get { return _customGames; } set { _customGames = value; } }
    [Display("Number of singles", null)]
    public int NoSingles { get { return _noSingles; } set { _noSingles = value; } }
    [Display("Number of doubles", null)]
    public int NoDoubles { get { return _noDoubles; } set { _noDoubles = value; } }

    [Display("Team size", null)]
    public int TeamSize { get { return _teamSize; } set { _teamSize = value; } }

    public TournamentDetail()
    {

        _games = 1;
        _sets = 1;
        _noSingles = 2;
        _noDoubles = 2;
        _teamSize = 2;
    }

     public TournamentDetail(int noSingle, int noDoubles, int noSets)
    {

        _noSingles = 2;
        _noDoubles = 2;
        _sets = noSets;
    }
}