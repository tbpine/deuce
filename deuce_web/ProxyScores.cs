
using System.Data.Common;
using deuce;

public class ProxyScores
{

    /// <summary>
    /// Saves a list of scores to the database using the provided database connection.
    /// </summary>
    /// <param name="listOfScores">A list of <see cref="Score"/> objects to be saved.</param>
    /// <param name="dbconn">The database connection to be used for saving the scores.</param>
    public static async Task Save(int tournamentId, List<Score> formScores, int round, DbConnection dbconn)
    {
        //Use repo to save
        var dbRepo = new DbRepoScore(dbconn);
        int scoreRoundIdx = round;

        List<Score> dbScores = await ProxyScores.GetScores(tournamentId, scoreRoundIdx, dbconn);

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
    public static async Task<List<Score>> GetScores(int tournamentId, int round, DbConnection dbconn)
    {
        var dbRepo = new DbRepoScore(dbconn);
        var filter = new Filter() { TournamentId = tournamentId, Round = round };

        return await dbRepo.GetList(filter);
    }

    //clear scores for a tournament by
    //calling the stored procedure "sp_clear_score" with the tournament id as a parameter
    public static void ClearScores(int tournamentId, int roundIdx, DbConnection dbconn)
    {
        DbRepoScore dbRepo = new(dbconn);
        dbRepo.ClearScores(tournamentId, roundIdx);

    }

}