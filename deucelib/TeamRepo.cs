namespace deuce;
/// <summary>
/// Extract teams from the "RecordTeamPlayer" class.
/// </summary>
public class TeamRepo
{
    private List<RecordTeamPlayer> _source;
    public TeamRepo(List<RecordTeamPlayer> src)
    {
        _source = src;
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

            team.AddPlayer(player);


        }

        return teams;
    }
}