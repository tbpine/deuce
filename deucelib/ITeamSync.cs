using System.Data.Common;

namespace deuce;

public interface ITeamSync
{
    void Run(List<Team> source, List<Team> dest, Tournament tournament, DbConnection dbConnection);
}