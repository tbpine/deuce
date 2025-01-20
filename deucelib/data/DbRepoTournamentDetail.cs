using System.Data;
using System.Data.Common;
using System.Diagnostics;
using deuce.ext;

namespace deuce;

public class DbRepoTournamentDetail : DbRepoBase<TournamentDetail>
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------
    private readonly DbConnection _dbconn;
    

    /// <summary>
    /// Construct with a db connection to the target db.
    /// </summary>
    /// <param name="dbconn">Database connection</param>
    public DbRepoTournamentDetail(DbConnection dbconn, Organization organization)
    {
        _dbconn = dbconn;
    }

    public override async Task<List<TournamentDetail>> GetList(Filter filter)
    {
        //return collection
        List<TournamentDetail> list = new();

        await  _dbconn.CreateReaderStoreProcAsync("sp_get_tournament_detail", ["p_tour"], [filter.TournamentId],
        reader=>{
            int tournamentId = reader.Parse<int>("tournament");
            int entries = reader.Parse<int>("no_entries");
            int sets = reader.Parse<int>("sets");
            int games = reader.Parse<int>("games");
            int customGames = reader.Parse<int>("custom_games");
            int noSingles = reader.Parse<int>("no_singles");
            int noDoubles = reader.Parse<int>("no_doubles");
            int teamSize = reader.Parse<int>("team_size");

            list.Add(new TournamentDetail
            {
                TournamentId = tournamentId,
                NoEntries = entries,
                Sets = sets,
                Games = games,
                CustomGames = customGames,
                NoSingles = noSingles,
                NoDoubles = noDoubles,
                TeamSize = teamSize

            });

        });

     
        return list;
    }

    public override async Task SetAsync(TournamentDetail obj)
    {


        var localTran = _dbconn.BeginTransaction();
        DbCommand cmd = _dbconn.CreateCommandStoreProc("sp_set_tournament_detail",
        ["p_tournament","p_no_entries", "p_sets", "p_games", "p_custom_games","p_no_singles", "p_no_doubles", "p_team_size"],
        [obj.TournamentId,obj.NoEntries, obj.Sets,obj.Games, obj.CustomGames, obj.NoSingles,obj.NoDoubles,obj.TeamSize],
        localTran);

        try
        {
            object? objret = await cmd.ExecuteNonQueryAsync();
            localTran.Commit();

        }
        catch (Exception ex)
        {
            localTran.Rollback();
            Debug.WriteLine(ex.Message);
        }




    }

}