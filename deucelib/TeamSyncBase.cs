namespace deuce;
using System.Data.Common;
public class TeamSyncBase : ITeamSync
{
    /// <summary>
    /// Run the sync process
    /// </summary>
    /// <param name="source">Source teams</param>
    /// <param name="dest">Destination teams</param>
    /// <param name="tournament">Tournament</param>
    /// <param name="dbConnection">Database connection</param>
    public virtual void Run(List<Team> source, List<Team> dest, Tournament tournament, DbConnection dbConnection)
    {
        // Default implementation does nothing
        // Override in derived classes to implement specific sync logic
    }
}