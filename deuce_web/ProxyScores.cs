
using System.Data.Common;
using deuce;

public class ProxyScores
{

    /// <summary>
    /// Saves a list of scores to the database using the provided database connection.
    /// </summary>
    /// <param name="listOfScores">A list of <see cref="Score"/> objects to be saved.</param>
    /// <param name="dbconn">The database connection to be used for saving the scores.</param>
    public static void Save(List<Score> listOfScores, DbConnection dbconn)
    {
        //Use repo to save
        var dbRepo = new DbRepoScore(dbconn);
        //Start transaction
        foreach (var score in listOfScores) dbRepo.Set(score);

    }

    /// <summary>
    /// Retrieves a list of scores for a specific tournament from the database using the provided database connection.
    /// </summary>
    /// <param name="tournamentId"> Tournament identifier</param>
    /// <param name="dbconn">Database connection</param>
    /// <returns></returns>
    public static async Task<List<Score>> GetScores(int tournamentId, DbConnection dbconn)
    {
        var dbRepo = new DbRepoScore(dbconn);
        var filter = new Filter() { TournamentId = tournamentId };

        return await dbRepo.GetList(filter);
    }

    //clear scores for a tournament by
    //calling the stored procedure "sp_clear_score" with the tournament id as a parameter
    public static void ClearScores(int tournamentId, DbConnection dbconn)
    {
        DbRepoScore dbRepo = new(dbconn);
        dbRepo.ClearScores(tournamentId);

    }
 
}