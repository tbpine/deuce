
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
}