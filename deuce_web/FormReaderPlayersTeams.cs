using deuce;
using System.Diagnostics;
using System.Text.RegularExpressions;


/// <summary>
/// Converts form values into teams
/// </summary>
public class FormReaderPlayersTeams : FormReaderPlayersBase
{
    //to save new players

    /// <summary>
    /// Construct with player dbrepos
    /// </summary>
    public FormReaderPlayersTeams()
    {

    }

    /// <summary>
    /// Do the convertion of form values to
    /// teams
    /// </summary>
    /// <param name="form">Form reference</param>
    /// <param name="organization">Form reference</param>
    /// <param name="tournament">Tournament where player is registered</param>
    /// <returns>List of teams</returns>
    public override List<Team> Parse(IFormCollection form,  Tournament tournament)
    {
        //Result
        List<Team> teams = new();
        //State
        Team? currentTeam = null;

        ///Transforms form values to teams
        foreach (var kp in form)
        {

            //Path format 
            //team_(Index)_(Id)_player_(Index)_(TeamPlayerID)_new
            //Form value is id for existing players
            //and name for new players

            //Ignore the action value
            if (kp.Key == "action") continue;

            var matches = Regex.Match(kp.Key, @"^team_(\d+)_(\d+)(_d)*(_player_)*(\d+)*_*(\d+)*_*(\d+)*(_d)*");

            if (matches.Success)
            {
                string teamIdx = matches.Groups[1].Value;
                string strTeamId = matches.Groups[2].Value;
                string strDelete = matches.Groups[3].Value;
                string playerIdx = matches.Groups[5].Value;
                string strPlayerId = matches.Groups[6].Value;
                string strPlayerTeamId = matches.Groups[7].Value;

                bool deletPlayer = !string.IsNullOrEmpty(matches.Groups[9].Value);
                int playerId = int.TryParse(strPlayerId, out playerId) ? playerId : 0;

                if (!string.IsNullOrEmpty(teamIdx) && string.IsNullOrEmpty(playerIdx) &&
                string.IsNullOrEmpty(strDelete))
                {
                    //Team form variable
                    string? strVal = kp.Value;
                    int idxTeam = int.TryParse(teamIdx, out idxTeam) ? idxTeam : 0;
                    int teamId = int.TryParse(strTeamId, out teamId) ? teamId : 0;
                    Team? team = new();
                    team.Id = teamId;
                    team.Label = string.IsNullOrEmpty(strVal) ? "" : strVal;
                    team.Index = idxTeam;

                    currentTeam = team;
                    teams.Add(currentTeam);
                    Debug.Print($"AdaptorFormTeams: {kp.Key}={kp.Value}. Team|name:{strVal}|idx:{idxTeam}|id:{teamId}");

                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx))
                {
                    //Check if they selected a registered player
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;
                    int playerTeamId = int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;

                    Player player = new Player()
                    {
                        Id = playerId,
                        Index = idxPlayer,
                        TeamPlayerId = playerTeamId,
                        Tournament = null

                    };

                    currentTeam?.AddPlayer(player);
                    Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}|idx:{idxPlayer}|tpid:{playerTeamId}");
                }
            }
        }
        //Validations

        //Check for teams with zero players
        List<Team> removeTeamList = new();
        foreach (Team team in teams)
            //Don't add teams with no players
            if (team.Players.Count() == 0) removeTeamList.Add(team);

        //Remove empty teams
        foreach (Team rmTeam in removeTeamList) teams.Remove(rmTeam);

        return teams;
    }


    /// <summary>
    /// Get the integer value of a form variable.
    /// </summary>
    /// <param name="form">Reference to the form</param>
    /// <param name="key">Variable key</param>
    /// <returns>0 if the value is not an integer</returns>
    private int GetFormInt(IFormCollection form, string key)
    {
        string? str = form[key];
        int ival = int.TryParse(str ?? "", out ival) ? ival : 0;

        return ival;
    }

}