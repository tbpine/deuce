using System.Data.Common;


namespace deuce;
public class TournamentRepo
{
    private readonly DbConnection _dbconn;
    private readonly Tournament _tournament;
    private readonly Organization _organization;

    /// <summary>
    /// Save a tournament to the database, breaking
    /// down to matches per round.
    /// </summary>
    /// <param name="dbconn">The database connection</param>
    /// <param name="tournament">The tournament to save</param>
    /// <param name="org">The organization</param>
    public TournamentRepo(DbConnection dbconn, Tournament tournament, Organization org)
    {
        _dbconn = dbconn;
        _tournament = tournament;
        _organization = org;
    }

    public async Task<bool> Save()
    {
        //players , permutation and round, and tournament
        //Action
        //What is needed to save the tournament for recreation ?
        //Teams
        var dbRepoTeam = new DbRepoTeam(_dbconn)
        {
            Organization = _organization,
            Tournament = _tournament
        };

        foreach (Team team in _tournament.Teams!)
            await dbRepoTeam.SetAsync(team);
        //Save matches
        var dbrepo = new DbRepoMatch(_dbconn);

        for (int i = 0; i < _tournament.Schedule!.NoRounds; i++)
        {
            Round round = _tournament.Schedule.GetRoundAtIndex(i);
            foreach (Permutation p in round.Permutations)
            {
                foreach (Match match in p.Matches)
                {
                    if (dbrepo is not null)
                        await dbrepo.SetAsync(match);
                }
            }
        }

        return true;
    }
}