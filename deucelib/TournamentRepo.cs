using System.Data.Common;
using deuce.lib;

namespace deuce;
public class TournamentRepo
{
    private readonly DbConnection _dbconn;
    private readonly Tournament _tournament;
    private readonly Organization _club;

    public TournamentRepo(DbConnection dbconn, Tournament tournament, Organization club)
    {
        _dbconn = dbconn;
        _tournament = tournament;
        _club = club;
    }

    public async Task<bool> Save()
    {
        //players , permutation and round, and tournament
        //Action
        //What is needed to save the tournament for recreation ?
        //Teams
        var dbRepoTeam = new DbRepoTeam(_dbconn)
        {
            Club = _club,
            Tournament = _tournament
        };

        foreach (Team team in _tournament.Teams!)
            await dbRepoTeam.Set(team);
        //Save matches
        var dbrepo = FactoryCreateDbRepo.Create<Match>(_dbconn);

        for (int i = 0; i < _tournament.Schedule!.NoRounds; i++)
        {
            Round round = _tournament.Schedule.GetRoundAtIndex(i);
            foreach (Permutation p in round.Permutations)
            {
                foreach (Match match in p.Matches)
                {
                    if (dbrepo is not null)
                        await dbrepo.Set(match);
                }
            }
        }

        return true;
    }
}