using System.Net;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using Org.BouncyCastle.Crypto.Modes.Gcm;

namespace deuce.ext;
public static class TeamExt
{

    /// <summary>
    /// Returns a csv of players in a team.
    /// </summary>
    /// <param name="team">Team containing players</param>
    /// <returns>csv of players in a team (unquoted) </returns>
    public static string GetPlayerCSV(this Team team)
    {
        StringBuilder sb = new();
        foreach (Player player in team.Players) sb.Append(player.ToString() + ",");
        sb.Remove(sb.Length - 1, 1);

        return sb.ToString();

    }

    /// <summary>
    /// Returns a csv of players in a team.
    /// </summary>
    /// <param name="team">Team containing players</param>
    /// <returns>csv of players in a team (unquoted) </returns>
    public static bool HasPlayer(this Team team, Player find)
    {
        foreach(Player player in team.Players)
        {
            if (player.Id == find.Id)  return true;
        }

        return false;

    }

    

}