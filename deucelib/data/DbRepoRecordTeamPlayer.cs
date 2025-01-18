using System.Data.Common;
using System.Diagnostics;
using deuce.ext;
using iText.StyledXmlParser.Jsoup.Nodes;

namespace deuce;
public class DbRepoRecordTeamPlayer : DbRepoBase<RecordTeamPlayer>
{
    private readonly DbConnection _dbconn;
    public DbRepoRecordTeamPlayer(DbConnection dbconn)
    {
        _dbconn = dbconn;
    }

    /// <summary>
    /// get a list of players from a club
    /// </summary>
    /// <param name="filter">Filter object</param>
    /// <returns></returns>
    public override async Task<List<RecordTeamPlayer>> GetList(Filter filter)
    {
        List<RecordTeamPlayer> list = new();
        await _dbconn.CreateReaderStoreProcAsync("sp_get_team_player", ["p_tournament"],
         [filter.TournamentId], reader=>{
                RecordTeamPlayer recordTeamPlayer = new(
                    reader.Parse<int>("id"),
                    reader.Parse<int>("organization"),
                    reader.Parse<int>("tournament"),
                    reader.Parse<string>("team"),
                    reader.Parse<int>("team_id"),
                    reader.Parse<int>("team_index"),
                    reader.Parse<int>("player_id"),
                    reader.Parse<int>("player_index"),
                    reader.Parse<string>("first_name"),
                    reader.Parse<string>("last_name"),
                    reader.Parse<double>("utr"),
                    reader.Parse<int>("team_player_id")
                );

                list.Add(recordTeamPlayer);

         });
        return list;
        
    }

}