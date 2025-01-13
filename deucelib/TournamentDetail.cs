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

    public int TournamentId { get { return _tournamentId; } set { _tournamentId = value; } }
    public int NoEntries { get { return _noEntries; } set { _noEntries = value; } }
    public int Sets { get { return _sets; } set { _sets = value; } }
    public int Games { get { return _games; } set { _games = value; } }
    public int CustomGames { get { return _customGames; } set { _customGames = value; } }
    public int NoSingles { get { return _noSingles; } set { _noSingles = value; } }
    public int NoDoubles { get { return _noDoubles; } set { _noDoubles = value; } }


}