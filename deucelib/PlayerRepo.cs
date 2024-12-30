namespace deuce;
/// <summary>
/// Extract players from the "RecordTeamPlayer" class.
/// </summary>
public class PlayerRepo
{
    public PlayerRepo()
    {

    }

    public List<Player> ExtractFromRecordTeamPlayer(List<RecordTeamPlayer> source)
    {
        List<Player> players = new();
        foreach(RecordTeamPlayer rteamPlayer in source)
        {
            Player player = new Player()
            {
                Id = rteamPlayer.PlayerId,
                First = rteamPlayer.FirstName,
                Last = rteamPlayer.LastName,
                Ranking = rteamPlayer.UTR
            };

            players.Add(player);
        }

        return players;
    }
}