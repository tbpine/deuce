namespace deuce.lib.ext;

/// <summary>
/// Player class extensions.
/// </summary>
public static class PlayerExt
{
    /// <summary>
    /// Find players not yet matched given a player's
    /// history
    /// </summary>
    /// <param name="player">Player in question</param>
    /// <param name="all">All players</param>
    /// <returns></returns>
    public static List<Player> ExcList(this Player player, List<Player> all)
    {
        //Get everone involved in this player's games.
        List<Player> tmp = new();
        foreach (var g in player.Games)
        {
            foreach (var p in g.Players)
                if (!tmp.Contains(p)) tmp.Add(p);
        }

        return all.FindAll(x => !tmp.Contains(x) && x.Id != player.Id);


    }
}