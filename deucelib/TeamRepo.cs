using System.Data.Common;
using System.Threading.Tasks;

namespace deuce;
/// <summary>
/// Extract teams from the "RecordTeamPlayer" class.
/// </summary>
public class TeamRepo
{
    private List<RecordTeamPlayer> _source;
    private Tournament? _tournament;
    private DbConnection? _dbconn;
    public TeamRepo(List<RecordTeamPlayer> src)
    {
        _source = src;
    }


    /// <summary>
    /// Load tournament entries.
    /// </summary>
    /// <param name="tournament">Tournament to load</param>
    /// <param name="dbconn">Database connection</param>
    public TeamRepo(Tournament? tournament, DbConnection? dbconn)
    {
        _tournament = tournament;
        _source = new();
        _dbconn = dbconn;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="players"></param>
    /// <returns></returns>
    public List<Team> ExtractFromRecordTeamPlayer()
    {
        List<Team> teams = new();
        foreach (RecordTeamPlayer rteamPlayer in _source)
        {
            Team? team = teams.Find(e => e.Id == rteamPlayer.TeamId);
            if (team is null)
            {
                team = new Team()
                {
                    Id = rteamPlayer.TeamId,
                    Label = rteamPlayer.Team,
                    Index = rteamPlayer.TeamIndex
                };

                teams.Add(team);
            }

            //Get players for this team
            Player player = new();
            //Soft link to the parent object
            player.Team = team;
            player.Id = rteamPlayer.PlayerId;
            player.Index = rteamPlayer.PlayerIndex;
            player.TeamPlayerId = rteamPlayer.TeamPlayerId;
            player.First = rteamPlayer.FirstName;
            player.Last = rteamPlayer.LastName;
            player.Member = new Member { Id = (rteamPlayer.Member?.Id??0)};
            team.AddPlayer(player);


        }

        return teams;
    }

    /// <summary>
    /// Who's playing in a tournament
    /// </summary>
    /// <typeparam name="Team">Team</typeparam>
    public async Task<List<Team>?> GetTournamentEntries()
    {
        //No db connnection or tournament was not specified
        if (_dbconn is null || _tournament is null)  return null;

        //Get the unnormalizes list of players fot the tournament
        DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer = new(_dbconn);
        Filter filter = new() { TournamentId = _tournament.Id};
        _source =  await dbRepoRecordTeamPlayer.GetList(filter);
        //Create the team player structure

        return ExtractFromRecordTeamPlayer();
    }
}