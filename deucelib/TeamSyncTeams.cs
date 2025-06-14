namespace deuce;
using System.Data.Common;
public class TeamSyncTeams : TeamSyncBase
{
    /// <summary>
    /// Run the sync process for teams
    /// </summary>
    /// <param name="source">Source teams</param>
    /// <param name="dest">Destination teams</param>
    /// <param name="tournament">Tournament</param>
    public override void Run(List<Team> source, List<Team> dest, Tournament tournament, DbConnection dbConnection)
    {
        // Ensure the database repository is initialized
        SyncMaster<Team> syncMaster = new(source, dest);
        //Make a DbRepoTeam object
        DbRepoTeam dbRepoTeam = new(dbConnection);
        dbRepoTeam.TournamentId = tournament.Id;
        dbRepoTeam.Organization = tournament.Organization;
        
        //Add handlers to insert, update and delete teams
        syncMaster.Add += (sender, team) =>
        {
            //Add team to db
            if (team is not null) dbRepoTeam.Set(team); ;
        };

        syncMaster.Update += (sender, team) =>
        {
            //Add team to db
            if (team is not null && team?.Dest is not null)
                dbRepoTeam.Set(team.Dest);
        };

        syncMaster.Remove += (sender, team) =>
        {
            //Add team to db
            if (team is not null) dbRepoTeam.Delete(team);
        };

        syncMaster.Run();
 
    }
}