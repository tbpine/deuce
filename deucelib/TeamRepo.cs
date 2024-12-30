namespace deuce;
/// <summary>
/// Extract teams from the "RecordTeamPlayer" class.
/// </summary>
public class TeamRepo
{
    public TeamRepo()
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="players"></param>
    /// <returns></returns>
    public List<Team> ExtractFromRecordTeamPlayer(List<RecordTeamPlayer> source, List<Player> players)
    {
        List<Team> teams = new();
        foreach (RecordTeamPlayer rteamPlayer in source)
        {
            Team? team = teams.Find(e => e.Id == rteamPlayer.TeamId);
            if (team is null)
            {
                team = new Team()
                {
                    Id = rteamPlayer.TeamId,
                    Label = rteamPlayer.Team
                };

                teams.Add(team);
            }

            //Get players for this team
            var player = players.Find(e => e.Id == rteamPlayer.PlayerId);
            if (player is not null) team.AddPlayer(player);


        }

        return teams;
    }
}