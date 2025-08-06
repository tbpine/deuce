namespace deuce;

using System.Data.Common;

public interface IScoreKeeper
{
  Task Save(int tournamentId, List<Score> formScores, int round, DbConnection dbconn);

  /// <summary>
  /// Retrieves a list of scores for a specific tournament from the database using the provided database connection.
  /// </summary>
  /// <param name="tournamentId"> Tournament identifier</param>
  /// <param name="dbconn">Database connection</param>
  /// <returns></returns>
  Task<List<Score>> GetScores(int tournamentId, int round, DbConnection dbconn);

  //clear scores for a tournament by
  //calling the stored procedure "sp_clear_score" with the tournament id as a parameter
  void ClearScores(int tournamentId, int roundIdx, DbConnection dbconn);

  Dictionary<Team, ScoringStats> GetRanking(Draw schedule,  List<Score> scores);

}
