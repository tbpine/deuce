using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using iText.Commons.Utils;

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
    /// <param name="round">Round object</param>
    /// <returns>List of matches</returns>
    public Permutation Create(Tournament t, Team home, Team away, int roundNo)
    {
        Permutation perm = new(roundNo, home, away);
        //Set up singles matches
        //TODO: Different ways to set up single
        var q1 = from p in home.Players orderby p.Ranking descending select p;
        var q2 = from p in away.Players orderby p.Ranking descending select p;

        Player[] a1 = q1.ToArray();
        Player[] a2 = q2.ToArray();


        TournamentDetail fmt = t.Details ?? new TournamentDetail()
        {
            NoSingles = 1,
            NoDoubles = 0,
            TeamSize = 1
        };

        Debug.Write($"|");

        for (int i = 0; i < fmt.NoSingles && i < (t.Details?.TeamSize ?? 0); i++)
        {
            Player pHome = a1[i];
            Player pAway = a2[i];
            Debug.Write($"({pHome},{pAway})");
            Match match = new Match("", roundNo) { PlayersPerSide = 1 };
            match.AddPairing(pHome, pAway);
            match.Permutation = perm;
            perm.AddMatch(match);

        }
        Debug.Write($"|");
        //Set up double matches
        //Ensure there's enough players, though
        for (int i = 0, j = 0; i < fmt.NoDoubles && (home.NoPlayers + away.NoPlayers) >= 4; i++, j += 2)
        {
            Player pHome1 = a1[j % home.NoPlayers];
            Player pHome2 = a1[(j + 1) % home.NoPlayers];
            Player pAway1 = a2[j % away.NoPlayers];
            Player pAway2 = a2[(j + 1) % away.NoPlayers];
            Debug.Write($"({pHome1} {pHome2},{pAway1} {pAway2})");
            Match match = new Match("", roundNo) { PlayersPerSide = 2 };
            match.AddHome(pHome1);
            match.AddHome(pHome2);
            match.AddAway(pAway1);
            match.AddAway(pAway2);

            match.Permutation = perm;
            perm.AddMatch(match);

        }


        return perm;
    }

    public List<Team> GetWinners(Permutation permutation, List<Score> scores)
    {
        //Create a list to hold the winners
        List<Team> winners = new List<Team>();
        //Use LINQ to group matches by away and home 
        //The key is this.This LINQ groups all macthes involving the same home and away teams
        var groupedMatches = from m in permutation.Matches
                             group m by new { Home = m.Home.FirstOrDefault()?.Team, Away = m.Away.FirstOrDefault()?.Team } into g
                             select new
                             {
                                 Teams = g.Key,
                                 Matches = g.ToList()
                             };

        //Create a dictionary to hold the team scores
        Dictionary<Team, ScoringStats> teamScores = new Dictionary<Team, ScoringStats>();
        //Add each team in the permutation to the dictionary
        foreach (var team in permutation.Teams)
        {
            if (!teamScores.ContainsKey(team))
                teamScores[team] = new ScoringStats(team);
        }

        foreach (var group in groupedMatches)
        {

            Team homeTeam = group.Teams.Home;
            Team awayTeam = group.Teams.Away;

            teamScores[homeTeam].Reset();
            teamScores[awayTeam].Reset();

            foreach (var match in group.Matches)
            {
                //Assuming scores are in the same order as matches
                //n sets
                List<Score> setScores = scores.FindAll(s => s.Match == match.Id && s.Permutation == permutation.Id);
                if (setScores?.Count > 0)
                {
                    var summaryHome = teamScores[homeTeam];
                    var summaryAway = teamScores[awayTeam];
                    //Count how many sets won by each team
                    int setsWonHome = 0;
                    int setsWonAway = 0;

                    foreach (var score in setScores)
                    {
                        summaryHome.AddScore(score, match);
                        summaryAway.AddScore(score, match);
                        setsWonHome += score.Home > score.Away ? 1 : 0;
                        setsWonAway += score.Away > score.Home ? 1 : 0;
                    }

                    //Update the summary for home and away teams
                    summaryHome.MatchesWon += setsWonHome > setsWonAway ? 1 : 0;
                    summaryHome.SetsWon += setsWonHome;

                    summaryAway.MatchesWon += setsWonAway > setsWonHome ? 1 : 0;
                    summaryAway.SetsWon += setsWonAway;


                }
            }

            //If the home team won more matched than the away team , then 
            //add it to the winners list
            if (teamScores[homeTeam].MatchesWon > teamScores[awayTeam].MatchesWon)
                winners.Add(homeTeam);
            else if (teamScores[homeTeam].MatchesWon < teamScores[awayTeam].MatchesWon)
                winners.Add(awayTeam);
            else
            {
                //If they won the same number of matches, then check the total score
                if (teamScores[homeTeam].TotalScore > teamScores[awayTeam].TotalScore)
                    winners.Add(homeTeam);
                else if (teamScores[homeTeam].TotalScore < teamScores[awayTeam].TotalScore)
                    winners.Add(awayTeam);
                else
                {
                    //If they are still equal, then it's a draw
                    winners.Add(homeTeam);
                    winners.Add(awayTeam);
                }
            }

        }

        return winners;
    }


}