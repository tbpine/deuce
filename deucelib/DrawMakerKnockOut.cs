using System.Diagnostics;
using deuce.ext;

namespace deuce;

/// <summary>
/// Implements a knockout (single-elimination) tournament scheduler.
/// In a knockout tournament, teams are eliminated after losing a single match,
/// with winners advancing to the next round until only one team remains.
/// </summary>
/// <remarks>
/// The scheduler automatically adds "BYE" teams if the number of participating teams
/// is not a power of 2, ensuring proper bracket structure. Teams are ranked and
/// paired such that the highest-ranked team plays the lowest-ranked team in the first round.
/// </remarks>
class DrawMakerKnockOut : DrawMakerBase, IDrawMaker
{
    /// <summary>
    /// The game maker instance used to create matches and permutations for the tournament.
    /// </summary>
    private readonly IGameMaker _gameMaker;

    //Keep a record of the last round winners and losers
    private List<Team> _winners = new();
    private List<Team> _losers = new();

    //Accessors for winners and losers
    public List<Team> Winners => _winners;
    public List<Team> Losers => _losers;

    /// <summary>
    /// Initializes a new instance of the SchedulerKnockOut class.
    /// </summary>
    /// <param name="t">The tournament for which to create the knockout schedule.</param>
    /// <param name="gameMaker">The game maker instance used to create matches.</param>
    public DrawMakerKnockOut(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    /// <summary>
    /// Creates a complete knockout tournament schedule with all rounds and matches.
    /// </summary>
    /// <param name="teams">The list of teams participating in the tournament.</param>
    /// <returns>A complete Schedule object containing all rounds and matches for the knockout tournament.</returns>
    /// <remarks>
    /// The method performs the following steps:
    /// 1. Adds BYE teams if needed to make the total number a power of 2
    /// 2. Sorts teams by ranking in descending order
    /// 3. Creates the first round by pairing highest vs lowest ranked teams
    /// 4. Creates subsequent rounds with placeholder teams that will be filled as winners advance
    /// 5. Calculates appropriate round labels (Final, Semi Final, Quarter Final)
    /// </remarks>
    public Draw Run(List<Team> teams)
    {
        //The result
        Draw draw = new Draw(_tournament);

        //Assigns
        _teams = teams;
        
        // Add a byes for number of teams that is not a power of 2.
        // Work out the number of byes needed.
        // For example: 6 teams needs 2 byes to make 8 (next power of 2)
        int exponent = (int)Math.Ceiling(Math.Log2(_teams.Count));
        int noByes = (int)Math.Pow(2, exponent) - teams.Count;

        // Add noByes to the teams list
        for (int i = 0; i < noByes; i++)
        {
            Team emptyTeam = new Team();
            emptyTeam.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, _teams.Count + i);
            _teams.Add(emptyTeam);
        }

        //Sort teams by ranking decending
        _teams.Sort((x, y) => (int)(y.Ranking - x.Ranking));
        //Assign indices to teams for bracket positioning
        for (int i = 0; i < _teams.Count; i++) _teams[i].Index = i + 1;

        // Work out rounds in a knockout tournament
        // The number of rounds is log2(teams)
        // For example, 8 teams = 3 rounds, 16 teams = 4 rounds, etc.
        int noRounds = (int)Math.Log2(_teams.Count);
        
        // First round has half the number of permutations as the number of teams.
        int noPermutations = _teams.Count / 2;
        
        // For each permutation, an element in the top half of "_teams" 
        // plays against an element in the bottom half of "_teams".
        // This ensures highest ranked plays lowest ranked, etc.
        Debug.WriteLine($"ex:{exponent}|byes:{noByes}|teams:{_teams.Count}|r:{noRounds}|perms:{noPermutations}");
        // Create first round matches
        for (int i = 0; i < noPermutations; i++)
        {
            var home = _teams[i];
            var away = _teams[_teams.Count - i - 1];

            // Only create matches for tennis tournaments (Sport == 1)
            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, 1);
                permutation.Id = i;
                draw.AddPermutation(permutation, 1);
            }
        }

        // From round 2 to the final round
        // Create placeholder matches with empty teams that will be populated as winners advance
        for (int r = 2; r <= noRounds; r++)
        {
            // In a knockout tournament, each round has half the number of matches as the previous round.
            // For example, 8 teams = 4 matches in the first round, 2 matches in the second round, and 1 match in the final.
            Debug.Write($"Round {r}:");
            noPermutations = (int)(_teams.Count / Math.Pow(2, r));
            
            // Make a list of empty teams as placeholders for winners
            List<Team> emptyTeams = new List<Team>();
            // Create empty placeholder teams for this round
            for (int i = 0; i < noPermutations; i++)
            {
                var home = new Team(0, "") { Index = i };
                var away = new Team(0, "") { Index = i + 1 };
                
                // Add placeholder players to maintain team structure
                for (int j = 0; j < _tournament.Details.TeamSize; j++)
                {
                    home.AddPlayer(new Player() { Index = j });
                    away.AddPlayer(new Player() { Index = j });
                }
                emptyTeams.Add(home);
                emptyTeams.Add(away);
            }

            // Create matches for this round using placeholder teams
            for (int p = 0; p < noPermutations; p++)
            {
                var home = emptyTeams[p];
                var away = emptyTeams[emptyTeams.Count - p - 1];
                Debug.Write("(" + home.Index + "," + away.Index + ")");

                // Schedule matches between each team (only for tennis tournaments)
                if (_tournament.Sport == 1)
                {
                    var permutation = _gameMaker.Create(_tournament, home, away, r);
                    permutation.Id = p;
                    // Add round labels for finals, semi-finals, etc.
                    draw.AddPermutation(permutation, r, GetRoundLabel(noRounds, r));
                }
            }

            Debug.Write($"\n");
        }




        return draw;
    }

    /// <summary>
    /// Specific to knockout tournaments. Get Final  , Quarters, semis....
    /// </summary>
    /// <param name="totalRounds">The total number of rounds in the tournament.</param>
    /// <param name="currentRound">The current round number for which to get the label.</param>
    /// <returns>
    /// A string label for the round:
    /// - "Final" for the last round
    /// - "Semi Final" for the second-to-last round (if total rounds > 3)
    /// - "Quarter Final" for the third-to-last round (if total rounds > 3)
    /// - Empty string for other rounds
    /// </returns>
    /// <remarks>
    /// Only provides labels for tournaments with more than 3 rounds to avoid
    /// labeling small tournaments inappropriately.
    /// </remarks>
    private string GetRoundLabel(int totalRounds, int currentRound)
    {
        if (totalRounds <= 3)
        {
            if (currentRound == totalRounds) return "Final";
            else
                return String.Empty;
        }
        else
        {
            if (currentRound == totalRounds) return "Final";
            else if (currentRound == totalRounds - 1) return "Semi Final";
            else if (currentRound == totalRounds - 2) return "Quarter Final";
        }

        return String.Empty;
    }

    /// <summary>
    /// Called before a round ends. Currently has no implementation for knockout tournaments.
    /// </summary>
    /// <param name="schedule">The tournament schedule.</param>
    /// <param name="round">The round number that is ending.</param>
    /// <param name="scores">The scores recorded for the round.</param>
    /// <remarks>
    /// This method is part of the IScheduler interface but knockout tournaments
    /// don't require any special processing before a round ends.
    /// </remarks>
    public void BeforeEndRound(Draw schedule, int round, List<Score> scores)
    {
        //Nothing to do here
    }


    /// <summary>
    /// Advances winners from the previous round to the next round in the knockout tournament.
    /// </summary>
    /// <param name="schedule">The tournament schedule containing all rounds and matches.</param>
    /// <param name="round">The current round number to populate with winners.</param>
    /// <param name="previousRound">The previous round number from which to determine winners.</param>
    /// <param name="scores">The scores used to determine match winners.</param>
    /// <remarks>
    /// This method:
    /// 1. Determines the winner of each match in the previous round based on scores
    /// 2. Places winners in the appropriate positions in the current round
    /// 3. Updates match participants with the advancing teams
    /// 4. Uses tennis scoring rules (sets won, then total games) to determine winners
    /// 
    /// The positioning logic ensures that winners from adjacent matches in the previous round
    /// are placed to face each other in the current round.
    /// </remarks>
    public void NextRound(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        //Clear previous winners and losers
        _winners.Clear();
        _losers.Clear();

        // Get the previous round
        Round? prevRound = schedule.Rounds.FirstOrDefault(r => r.Index == previousRound);
        if (prevRound == null) return;

        // Calculate expected number of permutations for current round (half of previous round)
        int previousRoundPermutations = prevRound.Permutations.Count;
        int expectedCurrentRoundPermutations = previousRoundPermutations / 2;
        
        // Get current round or create if it doesn't exist
        Round? currentRound = schedule.Rounds.FirstOrDefault(r => r.Index == round);
        
        // Count current permutations in the round
        int currentRoundPermutations = currentRound?.Permutations.Count ?? 0;

        // Determine winners from previous round based on scores and update current round
        for(int i = 0; i < prevRound.Permutations.Count; i++) 
        {
            var permutation = prevRound.GetAtIndex(i);
            // In knockout tournaments, each permutation has exactly one match
            var match = permutation?.Matches.FirstOrDefault();
            if (match != null)
            {
                // Find all scores for this match across all sets
                var matchScores = scores.Where(s => s.Match == match.Id && s.Round == previousRound).ToList();
                
                if (matchScores.Any())
                {
                    // Determine winner based on sets won (tennis scoring)
                    Team? winner = DetermineMatchWinner(matchScores, match);
                    
                    if (winner != null)
                    {
                        //Store winner and loser
                        _winners.Add(winner);
                        var losingTeam = match.GetLosingSide(winner);
                        if (losingTeam != null) _losers.Add(losingTeam);

                        // Calculate which permutation in the current round this winner should go to
                        // Adjacent matches in previous round feed into the same match in current round

                        int currentRoundPermIndex = i % 2 > 0 ? (i - 1) / 2 : i / 2;

                        // Find the permutation in the current round
                        var currentRoundPerm = currentRound?.GetAtIndex(currentRoundPermIndex);
                        if (currentRoundPerm != null)
                        {
                            // Update the appropriate team based on previous round index
                            // Even indices (0,2,4...) go to home team, odd indices (1,3,5...) go to away team
                            var nextMatch = currentRoundPerm.Matches.FirstOrDefault();
                            if (i % 2 == 0)
                            {
                                // No remainder - update home team (index 0)
                                currentRoundPerm.ReplaceTeamAtIndex(0, winner);
                                //There should alwaye be a match here
                                nextMatch?.ClearHomePlayers();
                                //Add winners to the next match home team
                                foreach (var player in winner.Players) nextMatch?.AddHome(player);
                            }
                            else
                            {
                                // Remainder - update away team (index 1)
                                currentRoundPerm.ReplaceTeamAtIndex(1, winner);
                                //There should alwaye be a match here
                                nextMatch?.ClearAwayPlayers();
                                //Add winners to the next match away team
                                foreach (var player in winner.Players) nextMatch?.AddAway(player);
                            }

                        }
                    }
                }
                else
                {
                    // If no score available, we can't determine winners yet
                    // This could happen if the previous round isn't complete
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Determines the winner of a tennis match based on sets won.
    /// In tennis, the winner is the team that wins the most sets.
    /// If sets are tied, the winner is determined by total games won.
    /// </summary>
    /// <param name="matchScores">List of scores for all sets in the match. Each score represents one set.</param>
    /// <param name="match">The match containing the teams competing in the match.</param>
    /// <returns>
    /// The winning team from the match, or null if the match is completely tied.
    /// Returns the team at index 0 (home) or index 1 (away) from the match.
    /// </returns>
    /// <remarks>
    /// The winner determination follows standard tennis rules:
    /// 1. Primary criterion: Team that wins the most sets
    /// 2. Tiebreaker: If sets are equal, team with most total games won
    /// 3. If both sets and games are equal, returns null (complete tie)
    /// 
    /// Each Score object in matchScores represents one set, with Home and Away
    /// properties indicating games won by each team in that set.
    /// </remarks>
    private Team? DetermineMatchWinner(List<Score> matchScores, Match match)
    {
        if (!matchScores.Any()) return null;

        int homeSetsWon = 0;
        int awaySetsWon = 0;
        int homeTotalGames = 0;
        int awayTotalGames = 0;

        // Count sets won by each team and total games
        foreach (var score in matchScores)
        {
            // Add games to total count
            homeTotalGames += score.Home;
            awayTotalGames += score.Away;
            
            // Count sets won
            if (score.Home > score.Away)
            {
                homeSetsWon++;
            }
            else if (score.Away > score.Home)
            {
                awaySetsWon++;
            }
            // Tied sets don't count toward either team
        }

        // First, determine winner based on sets won
        if (homeSetsWon > awaySetsWon)
        {
            return match.Home.FirstOrDefault()?.Team;
        }
        else if (awaySetsWon > homeSetsWon)
        {
            return match.Away.FirstOrDefault()?.Team;
        }
        else
        {
            // Sets are tied, determine winner by total games won
            if (homeTotalGames > awayTotalGames)
            {
                return match.Home.FirstOrDefault()?.Team; // Home team wins on games
            }
            else if (awayTotalGames > homeTotalGames)
            {
                return match.Away.FirstOrDefault()?.Team; // Away team wins on games
            }
        }
        
        // If completely tied (sets and games), return null
        return null;
    }
}