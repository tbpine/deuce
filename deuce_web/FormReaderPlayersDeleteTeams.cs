using deuce;
using System.Diagnostics;
using System.Text.RegularExpressions;


/// <summary>
/// Converts form values into teams
/// </summary>
class FormReaderPlayersDeleteTeams : FormReaderPlayersBase
{
    //to save new players

    /// <summary>
    /// Construct with player dbrepos
    /// </summary>
    public FormReaderPlayersDeleteTeams()
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
        //State
        Team? currentTeam = null;
        //Teams marked for deletion
        List<Team> delList = new();

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
                !string.IsNullOrEmpty(strDelete) && kp.Value == "on")
                {

                    //Delete team
                    int idxTeam = int.TryParse(teamIdx, out idxTeam) ? idxTeam : 0;
                    int teamId = int.TryParse(strTeamId, out teamId) ? teamId : 0;
                    //Team DTO for deletion
                    Team? delTeam = new();
                    delTeam.Id = teamId;
                    delTeam.Index = idxTeam;
                    currentTeam = delTeam;
                    //Add to the list of teams to delete 
                    delList.Add(delTeam);   


                }
                
                else if (!string.IsNullOrEmpty(teamIdx) && string.IsNullOrEmpty(playerIdx) &&
                !string.IsNullOrEmpty(strDelete) && string.IsNullOrEmpty(kp.Value))
                {
                    //Non deletion
                    currentTeam = null;
                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx)
                && (currentTeam is not null))
                {
                    //Check if they selected a registered player
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;
                    int playerTeamId = int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;

                    Player player = new Player()
                    {
                        Id = playerId,
                        Index = idxPlayer,
                        TeamPlayerId = playerTeamId,
                        Tournament = tournament
                    };

                    currentTeam?.AddPlayer(player);
                    Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}|idx:{idxPlayer}|tpid:{playerTeamId}");
                }
            }
        }

        return delList;
    }



}