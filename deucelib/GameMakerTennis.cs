using System.Diagnostics;
using System.Runtime.InteropServices;

namespace deuce;

/// <summary>
/// 
/// </summary>
public class GameMakerTennis : IGameMaker
{
    /// <summary>
    /// Tennis matches
    /// </summary>
    /// <param name="t">Tournament</param>
    /// <param name="home">Home team</param>
    /// <param name="away">Away team</param>
    /// <param name="round">Round</param>
    /// <returns>List of matches</returns>
    public List<Match> Create(Tournament t, Team home, Team away, int round)
    {
        //Set up singles matches
        //TODO: Different ways to set up single
        var q1 = from p in home.Players orderby p.Ranking descending select p;
        var q2 = from p in away.Players orderby p.Ranking descending select p;

        Player[] a1 = q1.ToArray();
        Player[] a2 = q2.ToArray();

        List<Match> results = new();

        Format fmt = t.Format??new Format(1, 0, 1);
        Debug.Write($"|");

        for (int i = 0; i < fmt.NoSingles && i  < t.TeamSize; i++)
        {
            Player pHome = a1[i];
            Player pAway = a2[i];
            Debug.Write($"({pHome},{pAway})");
            results.Add(new Match("", round, pHome, pAway ));

        }
        Debug.Write($"|");
        //Set up double matches
        //Ensure there's enough players, though
        for (int i = 0, j =0 ; i < fmt.NoDoubles  && (home.NoPlayers + away.NoPlayers) >= 4; i++, j+=2)
        {
            Player pHome1 = a1[j % home.NoPlayers];
            Player pHome2 = a1[ (j+1) % home.NoPlayers];
            Player pAway1 = a2[j % home.NoPlayers];
            Player pAway2 = a2[ (j+1) % home.NoPlayers];
            Debug.Write($"({pHome1} {pHome2},{pAway1} {pAway2})");
            results.Add(new Match("", round, pHome1, pHome2, pAway1, pAway2 ));

        }


        return results;
    }
}