namespace deuce;

public class TournamentDetail
{
    private int _tournamentId;
    private int _noEntries;
    private int _sets;
    private int _games;

    public int TournamentId { get { return _tournamentId; } set { _tournamentId = value; } }
    public int NoEntries { get { return _noEntries; } set { _noEntries = value; } }
    public int Sets { get { return _sets; } set { _sets = value; } }
    public int Games { get { return _games; } set { _games = value; } }


}