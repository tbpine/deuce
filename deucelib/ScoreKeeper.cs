
namespace deuce;

using System.Data.Common;
using deuce.ext;
public class ScoreKeeper : IScoreKeeper
{

    /// <summary>
    /// Saves a list of scores to the database using the provided database connection.
    /// </summary>
    /// <param name="listOfScores">A list of <see cref="Score"/> objects to be saved.</param>
    /// <param name="dbconn">The database connection to be used for saving the scores.</param>
    public async Task Save(int tournamentId, List<Score> formScores, int round, DbConnection dbconn)
    {
        //Use repo to save
        var dbRepo = new DbRepoScore(dbconn);
        int scoreRoundIdx = round;

        List<Score> dbScores = await GetScores(tournamentId, scoreRoundIdx, dbconn);

        SyncMaster<Score> syncMaster = new SyncMaster<Score>(formScores, dbScores);
        syncMaster.Add += (s, e) => { dbRepo.Set(e); };

        syncMaster.Update += (s, e) => { if (e.Source is not null) dbRepo.Set(e.Source); };
        syncMaster.Remove += (s, e) => { dbRepo.Delete(e); };

        syncMaster.Run();

    }

    /// <summary>
    /// Retrieves a list of scores for a specific tournament from the database using the provided database connection.
    /// </summary>
    /// <param name="tournamentId"> Tournament identifier</param>
    /// <param name="dbconn">Database connection</param>
    /// <returns></returns>
    public async Task<List<Score>> GetScores(int tournamentId, int round, DbConnection dbconn)
    {
        var dbRepo = new DbRepoScore(dbconn);
        var filter = new Filter() { TournamentId = tournamentId, Round = round };

        return await dbRepo.GetList(filter);
    }

    //clear scores for a tournament by
    //calling the stored procedure "sp_clear_score" with the tournament id as a parameter
    public void ClearScores(int tournamentId, int roundIdx, DbConnection dbconn)


    {
        DbRepoScore dbRepo = new(dbconn);
        dbRepo.ClearScores(tournamentId, roundIdx);

    }


    /// <summary>
    /// Calculates the ranking of teams based on their scores in a given schedule.
    /// The ranking is determined by the number of matches won, sets won, and total score.
    /// </summary>
    /// <param name="schedule">The schedule containing rounds and matches.</param>
    /// <param name="scores"> A list of scores for the matches in the schedule.</param>
    /// <returns></returns>
    public Dictionary<Team, ScoringStats> GetRanking(Schedule schedule, List<Score> scores)
    {
        //Create a dictionary to hold the team stats
        Dictionary<Team, ScoringStats> teamStats = new();

        //Loop through each round in the schedule
        foreach (var round in schedule.Rounds)
        {
            //Loop through each permutation in the round
            foreach (var permutation in round.Permutations)
            {
                //Loop through each match in the permutation
                foreach (var match in permutation.Matches)
                {
                    //Get the set scores for the match
                    var matchScores = scores.Where(s => s.Match == match.Id).ToList();
                    //Get the home team
                    var homeTeam = match.Home.FirstOrDefault()?.Team;
                    //Get the away team
                    var awayTeam = match.Away.FirstOrDefault()?.Team;
                    //If either team is null, skip this match
                    if (homeTeam is null || awayTeam is null) continue;

                    //Create a new ScoringStats for the home team if it doesn't exist
                    if (!teamStats.ContainsKey(homeTeam)) teamStats[homeTeam] = new ScoringStats();
                    //Create a new ScoringStats for the away team if it doesn't exist
                    if (!teamStats.ContainsKey(awayTeam)) teamStats[awayTeam] = new ScoringStats();
                    //Loop through each score in the match
                    foreach (var score in matchScores)
                    {
                        //update total score for each team
                        teamStats[homeTeam].TotalScore += score.Home;
                        teamStats[awayTeam].TotalScore += score.Away;
                        //update sets won for each team
                        if (score.Home > score.Away) teamStats[homeTeam].SetsWon++;
                        else if (score.Away > score.Home) teamStats[awayTeam].SetsWon++;
                    }

                    //Update matches won for each team
                    if (teamStats[homeTeam].SetsWon > teamStats[awayTeam].SetsWon) teamStats[homeTeam].MatchesWon++;
                    else if (teamStats[awayTeam].SetsWon > teamStats[homeTeam].SetsWon) teamStats[awayTeam].MatchesWon++;
                }
            }
        }

        //Sort by matches won, then by sets won, then by total score
        var sortedTeams = teamStats.OrderByDescending(ts => ts.Value.MatchesWon)
                                  .ThenByDescending(ts => ts.Value.SetsWon)
                                  .ThenByDescending(ts => ts.Value.TotalScore)
                                  .ToList();
        //Assign rankings based on the sorted order
        int rank = 1;
        foreach (var teamStat in sortedTeams)
            teamStat.Value.Ranking = rank++;

        return teamStats;

    }

}