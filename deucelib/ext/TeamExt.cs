using System.Net;
using System.Text;

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
}