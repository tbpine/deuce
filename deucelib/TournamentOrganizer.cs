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

        //keep track of created teams
        int newTeamIdx = 0;
        int newPlayerIdx = 0;
        
        for(int i = 0; i <  tournament.TeamSize; i++)
        {
            //Make sure there's enough players
        }

        //Has the tournament been scheduled ?

    }
}