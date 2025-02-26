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
        //Save tournament 
        DbRepoTournament dbRepoTournament = new(_dbconn, _organization);
        await dbRepoTournament.SetAsync(_tournament);
        //Save details ?
        TournamentDetail tourDetail = new();
        tourDetail.TournamentId = _tournament.Id;
        tourDetail.Sets = _tournament.Format?.NoSets??1;
        tourDetail.TeamSize = _tournament.TeamSize;
        tourDetail.NoDoubles = _tournament.Format?.NoDoubles??1;
        tourDetail.NoSingles = _tournament.Format?.NoSingles??1;
        tourDetail.NoEntries = (_tournament.Teams?.Count??2) * _tournament.TeamSize;

        DbRepoTournamentDetail dbRepoTourDetail = new(_dbconn);
        await dbRepoTourDetail.SetAsync(tourDetail);

        //players , permutation and round, and tournament
        //Action
        //What is needed to save the tournament for recreation ?
        //Sync Teams
        var dbRepoTeam = new DbRepoTeam(_dbconn, _organization, _tournament.Id);
        await dbRepoTeam.Sync(_tournament.Teams ?? new List<Team>());

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