using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Transactions;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentDetail : DbRepoBase<TournamentDetail>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;
    private readonly Organization? _organization;

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournamentDetail(DbConnection dbconn, Organization organization)
    {
        _dbconn = dbconn;
        _organization = organization;
    }

    public override async Task<List<TournamentDetail>> GetList(Filter filter)
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_get_tournament_detail";
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add(cmd.CreateWithValue("p_tour", filter.TournamentId));

        DbDataReader reader = await cmd.ExecuteReaderAsync();
        List<TournamentDetail> list = new();

        while (reader.Read())
        {
            int tournamentId = reader.Parse<int>("tournament");
            int entries = reader.Parse<int>("no_entries");
            int sets = reader.Parse<int>("sets");
            int games = reader.Parse<int>("games");
            int customGames = reader.Parse<int>("custom_games");
            int noSingles = reader.Parse<int>("no_singles");
            int noDoubles = reader.Parse<int>("no_doubles");

            list.Add(new TournamentDetail
            {
                TournamentId = tournamentId,
                NoEntries = entries,
                Sets = sets,
                Games = games,
                CustomGames = customGames,
                NoSingles = noSingles,
                NoDoubles = noDoubles

            });
        }
        reader.Close();
        return list;
    }

    public override async Task Set(TournamentDetail obj)
    {
        DbCommand cmd = _dbconn.CreateCommand();
        cmd.CommandText = "sp_set_tournament_detail";
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.Add(cmd.CreateWithValue("p_tournament", obj.TournamentId));
        cmd.Parameters.Add(cmd.CreateWithValue("p_no_entries", obj.NoEntries));
        cmd.Parameters.Add(cmd.CreateWithValue("p_sets", obj.Sets));
        cmd.Parameters.Add(cmd.CreateWithValue("p_games", obj.Games));
        cmd.Parameters.Add(cmd.CreateWithValue("p_custom_games", obj.CustomGames));
        cmd.Parameters.Add(cmd.CreateWithValue("p_no_singles", obj.NoSingles));
        cmd.Parameters.Add(cmd.CreateWithValue("p_no_doubles", obj.NoDoubles));
        var localTran = _dbconn.BeginTransaction();
        object? objret = null;
        try
        {
            objret = await cmd.ExecuteNonQueryAsync();
            localTran.Commit();

        }
        catch (Exception ex)
        {
            localTran.Rollback();
            Debug.WriteLine(ex.Message);
        }




    }

}