using System.Data.Common;
using System.Runtime.InteropServices;
using deuce.lib;

namespace deuce;

/// <summary>
/// Build a schedule
/// </summary>
public class BuilderSchedule
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
    public BuilderSchedule(List<RecordSchedule> records, List<Player> players, List<Team> teams,
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
    public Schedule Create()
    {
        //Keep state and iterate through each match
        StateBuilderSchedule state = new();
        Schedule schedule = new(_tournament!);

        for(int i = 0; i < _records?.Count; i++)
        {
            RecordSchedule recordMatch  = _records[i];
            //Current round
            if (state.Round is null ||  state.Round?.Index != recordMatch.Round) 
            {
                state.Round = new Round(recordMatch.Round);
                //Reset state
                state.Permutation = null;
                state.Match = null;
            }
            
            if (state.Permutation is null || state.Permutation.Id  !=  recordMatch.Permutation)
            {
                //Change in permutation
                state.Permutation =  new Permutation(recordMatch.Permutation);
                schedule.AddPermutation(state.Permutation, state.Round.Index);
            }

            if (state.Match is null || state.Match.Id != recordMatch.Match)
            {
                //Change in match
                state.Match = new Match(){
                    Id = recordMatch.Match,
                    Permutation = state.Permutation,
                    Round = state.Round.Index
                };

                state.Permutation.AddMatch(state.Match);

            }

            //Players
            if (recordMatch.PlayerAway > 0) 
            {
                Player? playerAway = _players?.Find(e=>e.Id == recordMatch.PlayerAway);
                if (playerAway != null) state.Match.AddAway(playerAway);
            }
            else if (recordMatch.PlayerHome > 0)
            {
                Player? playerHome = _players?.Find(e=>e.Id == recordMatch.PlayerHome);
                if (playerHome != null) state.Match.AddHome(playerHome);
            }


        }       

        //Last set of games
        

        return schedule;
    }
}