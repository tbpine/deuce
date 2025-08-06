using System.Data.Common;
using System.Diagnostics;
using Org.BouncyCastle.Asn1.Eac;

namespace deuce;

/// <summary>
/// Build a schedule
/// </summary>
public class BuilderDraws
{
    private readonly List<RecordSchedule>? _records;
    private readonly List<Player>? _players;
    private readonly List<Team>? _teams;
    private readonly Tournament? _tournament;
    private readonly DbConnection _dbconn;

    /// <summary>
    /// Construct with dependencies
    /// </summary>
    /// <param name="records">List of RecordSchedule objects</param>
    /// <param name="players">List of players</param>
    /// <param name="teams">List of teams</param>
    public BuilderDraws(List<RecordSchedule> records, List<Player> players, List<Team> teams,
    Tournament tournament, DbConnection dbconn)
    {
        _records = records;
        _players = players;
        _teams = teams;
        _tournament = tournament;
        _dbconn = dbconn;
    }

    /// <summary>
    /// Re-create a persisted tourament schedule
    /// </summary>
    /// <returns>Schedule object</returns>
    public Draw Create()
    {
        Debug.Assert(_players?.Count > 0);
        Debug.Assert(_teams?.Count > 0);
        Debug.Assert(_tournament is not null);
        Debug.Assert(_dbconn is not null);

        //Keep state and iterate through each match
        StateBuilderDraw state = new();
        Draw schedule = new(_tournament!);

        for (int i = 0; i < _records?.Count; i++)
        {
            RecordSchedule recordMatch = _records[i];
            //Current round
            if (state.Round is null || state.Round?.Index != recordMatch.Round)
            {
                state.Round = new Round(recordMatch.Round);
                
                //Reset state
                state.Permutation = null;
                state.Match = null;
            }

            if (state.Permutation is null || state.Permutation.Id != recordMatch.Permutation)
            {
                //Change in permutation
                state.Permutation = new Permutation(recordMatch.Permutation);
                schedule.AddPermutation(state.Permutation, state.Round.Index);


            }

            if (state.Match is null || state.Match.Id != recordMatch.Match)
            {
                //Change in match
                state.Match = new Match()
                {
                    Id = recordMatch.Match,
                    Permutation = state.Permutation,
                    Round = state.Round.Index,
                    PlayersPerSide = recordMatch.PlayersPerSide
                };

                state.Permutation.AddMatch(state.Match);

            }
            //A match can involve multiple players on each side.
            //Check home side has enough players.
            if ((state.Match.Home?.Count() ?? 0) < state.Match.PlayersPerSide)
            {
                Player playerHome = _players?.Find(e => e.Id == recordMatch.PlayerHome) ?? new Player();
                state.Match.AddHome(playerHome);
                
                //If a team exists in _team , then added it to the permutation
                //But, if the team does not exist, then create a new team with an empty label
                var team = state.Permutation.Teams.FirstOrDefault(e => e.Id == recordMatch.TeamHome);

                if (team is null)
                {
                    team = _teams.Find(e => e.Id == recordMatch.TeamHome);
                    if (team is null) team = new Team(recordMatch.TeamHome, "");

                    state.Permutation.AddTeam(team);
                }
                team.AddPlayer(playerHome);
            }
            else if ((state.Match.Away?.Count() ?? 0) <  state.Match.PlayersPerSide)
            {
               Player playerAway = _players?.Find(e => e.Id == recordMatch.PlayerAway) ?? new Player();
                state.Match.AddAway(playerAway);
                
                //If a team exists in _team , then added it to the permutation
                //But, if the team does not exist, then create a new team with an empty label
                var team = state.Permutation.Teams.FirstOrDefault(e => e.Id == recordMatch.TeamAway);

                if (team is null)
                {
                    team = _teams.Find(e => e.Id == recordMatch.TeamAway);
                    if (team is null) team = new Team(recordMatch.TeamAway, "");

                    state.Permutation.AddTeam(team);
                }
                team.AddPlayer(playerAway);
            }

          
        }



        return schedule;
    }

}