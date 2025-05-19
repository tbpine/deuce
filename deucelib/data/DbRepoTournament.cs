using System.Data.Common;
using System.Diagnostics;
using deuce.ext;

namespace deuce;

public class DbRepoTournament : DbRepoBase<Tournament>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private  Organization _organization = new();

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournament(DbConnection dbconn) : base(dbconn)
    {
    }

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournament(DbConnection dbconn, Organization organization) : base(dbconn)
    {
        _organization = organization;
    }

    public override async Task<List<Tournament>> GetList(Filter filter)
    {
        _dbconn.Open();

        List<Tournament> list = new();

        //Don't bother loading tournament is 
        //no id

        if (filter.TournamentId < 1) return list;

        await _dbconn.CreateReaderStoreProcAsync("sp_get_tournament", ["p_id"], [filter.TournamentId],
        reader =>
        {
            int id = reader.Parse<int>("id");
            string label = reader.Parse<string>("label");
            DateTime start = reader.Parse<DateTime>("start");
            DateTime end = reader.Parse<DateTime>("end");

            int interval = reader.Parse<int>("interval");
            int steps = reader.Parse<int>("steps");
            int type = reader.Parse<int>("type");
            int max = reader.Parse<int>("max");

            float fee = reader.Parse<float>("fee");
            float prize = reader.Parse<float>("prize");
            bool useRanking = reader.Parse<int>("seedings") == 1;
            int sportId = reader.Parse<int>("sport");
            int entryType = reader.Parse<int>("entry_type");
            int status  = reader.Parse<int>("status");

            list.Add(new Tournament
            {
                Id = id,
                Label = label,
                Start = start,
                End = end,
                Interval = interval,
                Steps = steps,
                Type = type,
                Max = max,
                Fee = fee,
                Prize = prize,
                UseRanking = useRanking,
                Sport = sportId,
                Organization = _organization,
                EntryType = entryType,
                Status = (TournamentStatus)status

            });

        });

        _dbconn.Close();

        return list;
    }

    public override async Task SetAsync(Tournament obj)
    {
        _dbconn.Open();

        var localTran = _dbconn.BeginTransaction();
        
        //Explicitly insert new rows if id < 1
        object primaryKeyId = obj.Id < 1 ? DBNull.Value : obj.Id;

        DbCommand cmd = _dbconn.CreateCommandStoreProc("sp_set_tournament",
        ["p_id", "p_label", "p_start", "p_end", "p_interval", "p_steps", "p_type", "p_max", "p_fee", "p_prize", "p_seedings", "p_sport", "p_organization", "p_entry_type", "p_status"],
        [primaryKeyId, obj.Label ?? "", obj.Start, obj.End, obj.Interval, obj.Steps, obj.Type, obj.Max, obj.Fee, obj.Prize, obj.UseRanking, obj.Sport, obj.Organization?.Id ?? 1, obj.EntryType < 1 ? 1 : obj.EntryType, obj.Status], localTran);

        try
        {
            obj.Id  =cmd.GetIntegerFromScaler(await cmd.ExecuteScalarAsync());

            localTran.Commit();

        }
        catch (DbException dex)
        {
            localTran.Rollback();
            //If it's a unique constriant then
            //throw an application exception
            
            Debug.WriteLine(dex.Message);
        }
        catch (Exception ex)
        {
            localTran.Rollback();
            Debug.WriteLine(ex.Message);
        }
        finally
        {
            _dbconn.Close();
        }



    }

}