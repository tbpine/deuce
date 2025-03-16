using System.Diagnostics;
using System.Runtime.InteropServices.Marshalling;
using System.Text.RegularExpressions;
using deuce;
using K4os.Compression.LZ4.Internal;

/// <summary>
/// Converts form values into teams
/// </summary>
class AdaptorFormTeams : AdaptorTeamsBase
{
    //to save new players

    /// <summary>
    /// Construct with player dbrepos
    /// </summary>
    public AdaptorFormTeams()
    {

    }

    /// <summary>
    /// Do the convertion of form values to
    /// teams
    /// </summary>
    /// <param name="form">Form reference</param>
    /// <param name="organization">Form reference</param>
    /// <returns>List of teams</returns>
    public override List<Team> Convert(IFormCollection form, Organization organization)
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

            var matches = Regex.Match(kp.Key, @"team_(\d+)_(\d+)(_d)*(_player_)*(\d+)*_*(\d+)*_*(\d+)*(_new)*(_d)*");

            if (matches.Success)
            {
                string teamIdx = matches.Groups[1].Value;
                string strTeamId = matches.Groups[2].Value;
                string strDelete = matches.Groups[3].Value;
                string playerIdx = matches.Groups[5].Value;
                string strPlayerId = matches.Groups[6].Value;
                string strPlayerTeamId = matches.Groups[7].Value;

                bool isNew = !string.IsNullOrEmpty(matches.Groups[8].Value);
                bool deletPlayer = !string.IsNullOrEmpty(matches.Groups[9].Value);
                int playerId = int.TryParse(strPlayerId, out playerId) ? playerId : 0;

                if (!string.IsNullOrEmpty(teamIdx) && string.IsNullOrEmpty(playerIdx) && !isNew)
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
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && !isNew )
                {
                    //Check if they selected a registered player
                    int memberId = this.GetFormInt(form, kp.Key);
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;
                    int playerTeamId = int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;
                    //Is there a player at this DISPLAYED index
                    var existingPlayer = currentTeam?.Find(p => p.Index == idxPlayer && playerTeamId == p.TeamPlayerId);
                    if (existingPlayer is not null && memberId > 0)
                    {
                        existingPlayer.Id = playerId;
                        //Update it's id
                        if (existingPlayer.Member is not null) existingPlayer.Member.Id = memberId;
                        Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}. {(existingPlayer is null ? "New" : "")} Player|id:{memberId}|idx:{idxPlayer}|tpid:{playerTeamId}");
                    }
                    else if (memberId > 0)
                    {
                        Player player = new Player()
                        {
                            Id = playerId,
                            Index = idxPlayer,
                            TeamPlayerId = playerTeamId,
                            Member = new() { Id = memberId }
                        };

                        currentTeam?.AddPlayer(player);
                        Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}. {(existingPlayer is null ? "New" : "")} Player|id:{memberId}|idx:{idxPlayer}|tpid:{playerTeamId}");
                    }
                }
                else if (!string.IsNullOrEmpty(teamIdx) && !string.IsNullOrEmpty(playerIdx) && isNew) //..&&
                //!string.IsNullOrEmpty(kp.Value))
                {
                    //Unspecified players are handled as 
                    //non members


                    string? strval = kp.Value;
                    int playerTeamId = int.TryParse(strPlayerTeamId, out playerTeamId) ? playerTeamId : 0;
                    //Split first and last names
                    string[] names = (strval?.Trim() ?? "").Split(" ");
                    string firstname = names.Length > 0 ? names[0].Trim() : "";
                    string lastname = names.Length > 1 ? names[1].Trim() : "";
                    int idxPlayer = int.TryParse(playerIdx, out idxPlayer) ? idxPlayer : 0;

                    var existingPlayer = currentTeam?.Find(p => p.Index == idxPlayer && playerTeamId == p.TeamPlayerId);
                    //Player exists at this index. 
                    //It could be a member or non member.
                    
                    if (existingPlayer is not null)
                    {
                        if (!string.IsNullOrEmpty(strval))
                        {
                            //Update it's id
                            existingPlayer.First = firstname;
                            existingPlayer.Last = lastname;
                            existingPlayer.Member = null;
                            existingPlayer.Id = playerId;

                            Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}. {(existingPlayer is null ? "New" : "")} Player|{firstname}|{lastname}|Idx:{idxPlayer}");
                        }
                    }
                    else
                    {
                        //New non member player specified
                        Player player = new Player()
                        {
                            Id = playerId,
                            First = firstname,
                            Last = lastname,
                            Index = idxPlayer,
                            TeamPlayerId = playerTeamId
                        };

                        currentTeam?.AddPlayer(player);
                        Debug.Print($@"AdaptorFormTeams: {kp.Key}={kp.Value}. {(existingPlayer is null ? "New" : "")} Player|{firstname}|{lastname}|Idx:{idxPlayer}");

                    }



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