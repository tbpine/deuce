namespace deuce.ext;

public static class TournamentExt
{
    /// <summary>
    /// Check if the tournament is new.
    /// </summary>
    /// <param name="tournament">Tournamen object</param>
    /// <returns></returns>
    public static bool IsNew(this Tournament tournament)
    {
        return tournament.Id == 0;
    }
}