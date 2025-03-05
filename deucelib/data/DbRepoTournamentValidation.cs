using System.Data.Common;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentValidation : DbRepoBase<ResultTournamentValidation>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
   /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournamentValidation(DbConnection dbconn) : base(dbconn)
    {
    }


    public override async Task<List<ResultTournamentValidation>> GetList(Filter filter)
    {
        List<ResultTournamentValidation> list = new();
        
        //Allow empty names
        //Can edit and put one in later.
        if (string.IsNullOrEmpty(filter.TournamentLabel))
        {
            list.Add(new ResultTournamentValidation("", true));
            return list;
        }

        _dbconn.Open();


        //Don't bother loading tournament is 
        //no id

        await _dbconn.CreateReaderStoreProcAsync("sp_validate_tournament", ["p_label"], [filter.TournamentLabel??""],
        reader =>
        {
            bool validLabel = reader.Parse<int>("labels") == 0;
            string message = validLabel ? "" : "Tournament with name exists. Choose another name";
            list.Add(new ResultTournamentValidation(message, validLabel));
        });

        _dbconn.Close();

        return list;
    }



}