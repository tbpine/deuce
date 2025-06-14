using deuce;
using System.Diagnostics;
using System.Text.RegularExpressions;


/// <summary>
/// Crate a team for each player
/// </summary>
public class FormReaderIndList : FormReaderPlayersBase
{
    //to save new players

    /// <summary>
    /// Add all players into individual teams of one
    /// </summary>
    public FormReaderIndList()
    {

    }

    /// <summary>
    /// Do the convertion of form values to
    /// teams
    /// </summary>
    /// <param name="form">Form reference</param>
    /// <param name="tournament">Tournament where player is registered</param>
    /// <returns>List of teams</returns>
    public override List<Team> Parse(IFormCollection form,  Tournament tournament)
    {
        //The new team created.
        List<Team> teams = new();
        //State
        ///Transforms form values to teams
        foreach (var kp in form)
        {

            //Path format 
            //player_id_firstname_lastname_middlename
            //Form value is id for existing players
            //and name for new players

            //Ignore the action value
            if (kp.Key == "action") continue;

            var matches = Regex.Match(kp.Key, @"^player_(\d+)_*([^_]+)*_*([^_]+)*_*([^_]+)*");
            
            if (matches.Success)
            {
                string player_id = matches.Groups[1].Value;
                string player_first = matches.Groups[2].Value;
                string player_last = matches.Groups[3].Value;
                string player_middle = matches.Groups[4].Value;

                if (!string.IsNullOrEmpty(player_id))
                {
                    Team newTeam = new Team();
                    newTeam.Index = teams.Count; //Set the team index to the player index
                   
                    Debug.Print($"AdaptorFormAddPlayer: {kp.Key}={kp.Value}. PlayerId|{player_id}|First:{player_first}|Last:{player_last}|Middle:{player_middle}");
                    //Use DTO, as we only know the player id
                    newTeam.AddPlayer(new Player()
                    {
                        Id = int.TryParse(player_id, out int playerId) ? playerId : 0,
                        First = player_first,
                        Last = player_last,
                        Index = 1,
                        Tournament = tournament
                    });
                    teams.Add(newTeam);
                }
            }
        }

        return teams;
    }


}