namespace deuce;

using System.Data.SqlTypes;
using deuce;

/// <summary>
/// Responsible for the correct create of a 
/// tournament
/// </summary>
public class TournamentOrganizer
{
    /// <summary>
    /// Given the parameters of a touranment, ensure
    /// that the correct tournament is created i.e
    /// Add the required players  and teams ( get database id 
    /// and create names). For round robin create rounds.
    /// </summary>
    /// <param name="tournament"></param>
    /// <param name="teams"></param>
    
    public void Run(Tournament tournament)
    {
        //Make sure that there's enough teams
        //Check the number of teams

        //keep track of created teams and players
        List<Team> teamHolders = new();
        List<Player> playerHolders = new();

        for(int i = 0; i <  tournament.TeamSize; i++)
        {
            //Make sure there's enough players
            if (tournament.Teams is not null && i >= tournament.Teams.Count)
            {
                //Add a new team
                Team newTeam = new Team() { Id = 0, Label = $"team {teamHolders.Count+1}"};
                //Add the required players

                for(int j = 0; j < tournament.TeamSize; j++)
                {
                    
                }

            }
        }

        //Has the tournament been scheduled ?

    }
}