using System.Data.Common;
using deuce.ext;

namespace deuce;
public class DbRepoVenue : DbRepoBase<TournamentVenue>
{
    private readonly DbConnection _dbconn;


    /// <summary>
    /// Constructure with db connectopon and venue
    /// </summary>
    /// <param name="dbconn"></param>
    /// 
    public DbRepoVenue(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// get a list of TournamentVenue for a tournament.
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<TournamentVenue>> GetList(Filter filter)
    {
        List<TournamentVenue> venues = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_tournament_venue", ["p_tour_id"], [filter.TournamentId],
        r =>
        {
            TournamentVenue venue = new()
            {
                Id = r.Parse<int>("id"),
                Street = r.Parse<string>("street"),
                Suburb = r.Parse<string>("suburb"),
                State = r.Parse<string>("state"),
                PostCode = r.Parse<int>("post_code"),
                Country =r.Parse<string>("country")
            };

            venues.Add(venue);

        });

        return venues;

    }

    public override void Set(TournamentVenue obj)
    {
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;
        //Start transaction
        var dbTrans = _dbconn.BeginTransaction();
        try
        {
            var command = _dbconn.CreateCommandStoreProc("sp_set_tournament_venue", ["p_id", "p_tournament", "p_street", "p_suburb",
            "p_state", "p_post_code", "p_country"], [primaryKeyId, obj.Tournament?.Id??0, obj.Street, obj.Suburb , obj.State,
            obj.PostCode, obj.Country ],dbTrans);


            obj.Id = command.GetIntegerFromScaler(command.ExecuteScalar());
            dbTrans.Commit();
        }
        catch (DbException)
        {
            //Something went wrong
            dbTrans.Rollback();
        }

    }
}