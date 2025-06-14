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
        foreach (Player player in team.Players)
        {
            sb.Append(player.ToString() + ",");
        }
        if (sb.Length>0) sb.Remove(sb.Length - 1, 1);

        return sb.ToString();

    }

    /// <summary>
    /// Returns a csv of players in a team.
    /// </summary>
    /// <param name="team">Team containing players</param>
    /// <returns>csv of players in a team (unquoted) </returns>
    public static bool HasPlayer(this Team team, Player find)
    {
        foreach (Player player in team.Players)
        {
            if (player.Id == find.Id) return true;
        }

        return false;

    }

    /// <summary>
    /// Creates a bye team with the same number of players as the tournament team size.
    /// </summary>
    /// <param name="team">Team object</param>
    /// <param name="teamSize">Tournament team size</param>
    /// <returns></returns>
    public static void CreateBye(this Team team, int teamSize, Organization org, int index)
    {

        team.Id = 0;
        team.Label = "BYE";
        team.Club = org;
        team.Index = index;

        //Add tournament team size number of new players to the bye team
        for (int i = 0; i < teamSize; i++)
        {
            Player player = new()
            {
                Id = 0,
                First = "BYE",
                Last = "BYE",
                Team = team,
                Bye = true
            };

            team.AddPlayer(player);
        }


    }
    


}