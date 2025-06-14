namespace deuce;
using System.Data.Common;
public class TeamSyncInd : TeamSyncBase
{
    /// <summary>
    /// Run the sync process for individual teams
    /// </summary>
    /// <param name="source">Source teams</param>
    /// <param name="dest">Destination teams</param>
    /// <param name="tournament">Tournament</param>
    public override void Run(List<Team> source, List<Team> dest, Tournament tournament, DbConnection dbConnection)
    {
        
        //Creae a DbRepoRecordTeamPlayer object with "dbConnection" as the connection
        DbRepoRecordTeamPlayer dbRepoRecordTeamPlayer = new(dbConnection);
        //List of teams to add
        List<Team> teamsToAdd = new();
        //List of teams to delete
        List<Team> teamsToDelete = new();

        //For each team in the source list, find the team containing the same players in the destination list
        foreach (Team sourceTeam in source)
        {
            //Find the team in the destination list with the same players
            Team? destTeam = dest.Find(e => e.Players.All(p => sourceTeam.Players.Any(sp => sp.Id == p.Id)));

            //If the team was not found, add it to the destination list
            if (destTeam is null) teamsToAdd.Add(sourceTeam);
        }

        //for each team in the destination list, find the team containing the same players in the source list
        foreach (Team destTeam in dest)
        {
            //Find the team in the source list with the same players
            Team? sourceTeam = source.Find(e => e.Players.All(p => destTeam.Players.Any(dp => dp.Id == p.Id)));

            //If the team was not found, add it to the teams to delete list
            if (sourceTeam is null) teamsToDelete.Add(destTeam);
        }

        //Create a DbRepoTeam object with "dbConnection" as the connection
        DbRepoTeam dbRepoTeam = new(dbConnection);
        dbRepoTeam.TournamentId = tournament.Id;
        dbRepoTeam.Organization = tournament.Organization;
        
        //Add the teams to the database
        foreach (Team team in teamsToAdd) dbRepoTeam.Set(team);
        //Delete the teams from the database
        foreach (Team team in teamsToDelete) dbRepoTeam.Delete(team);
    }
}