using System.Diagnostics;
using deuce.ext;

namespace deuce;

/// <summary>
/// For the swiss system. Teams play each round for points with
/// the winner having the highest points winning. Or, in the event of a tie-break, have configurable
/// ways to separate them.
/// 
/// For each round , standings are calculated : points are awarded for wins and draws.
/// Then , standings are sorted by points, and teams are paired accordingly for the next round.
/// Teams on the same points can be paired by:
/// - ranking
/// - bottom vs top
/// - random
/// - adjacent
/// 
/// Issues to consider:
///  - Repeated matches should be avoided if possible.
/// - Byes should be assigned fairly (e.g., to lowest-ranked teams that haven't had a bye yet).
/// 
///  - If a group has one player it is given a bye
///  - If a group has add odd number of players, the group has to be adjusted by adding
///  a lower score player.
/// </summary>
public class DrawMakerSwiss : DrawMakerBase, IDrawMaker
{
    private readonly IGameMaker _gameMaker;
    private readonly Dictionary<int, List<TeamStanding>> _standingsHistory;

    public DrawMakerSwiss(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
        _standingsHistory = new Dictionary<int, List<TeamStanding>>();
    }

    public override Draw Create()
    {
        //The result
        Draw draw = new Draw(_tournament);
        //Assigns
        var teams = _tournament.Teams;

        // Sort teams by ranking initially (higher ranked teams first)
        teams.Sort((x, y) => (int)(y.Ranking - x.Ranking));

        // Add indexes to teams (for this tournament).
        //Different from team primary key.
        for (int i = 0; i < teams.Count; i++)
            teams[i].Index = i + 1;

        // Calculate number of rounds based on tournament format or default
        // Swiss format typically uses log2(n) rounds, but can be customized
        int noRounds = CalculateNumberOfRounds(teams.Count);

        Debug.WriteLine($"Swiss Tournament: Teams:{teams.Count}, Rounds:{noRounds}");

        // Create first round with initial pairings
        CreateInitialRound(draw, 0);

        return draw;
    }

    /// <summary>
    /// Progress to the next round of the schedule.
    /// In Swiss format, we need to pair teams based on their current standings.
    /// </summary>
    /// <param name="schedule">The current schedule</param>
    /// <param name="round">The current round number</param>
    /// <param name="previousRound">The previous round number</param>
    /// <param name="scores">List of scores from recent matches</param>
    public override void OnChange(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        // Record standings for the completed round
        if (previousRound >= 0)
        {
            UpdateStandingsForCompletedRound(schedule, previousRound, scores);
        }

        // If this is not the first round, create the next round based on current standings
        if (round > 0 && round < CalculateNumberOfRounds(_tournament.Teams.Count()))
        {
            CreateNextRound(schedule, round, scores);
        }
    }

    /// <summary>
    /// Calculate the number of rounds for Swiss format.
    /// Typically uses log2(n) rounds, but can be customized based on tournament settings.
    /// </summary>
    /// <param name="teamCount">Number of teams in the tournament</param>
    /// <returns>Number of rounds to play</returns>
    private int CalculateNumberOfRounds(int teamCount)
    {
        if (teamCount <= 1) return 0;

        // Standard Swiss format uses ceil(log2(n)) rounds
        // But we can also use tournament settings if available
        int standardRounds = (int)Math.Ceiling(Math.Log2(teamCount)) + 1;

        // Cap at reasonable maximum (typically no more than the number of teams - 1)
        return Math.Min(standardRounds, teamCount - 1);
    }

    /// <summary>
    /// Create the initial round with teams paired by ranking.
    /// </summary>
    /// <param name="draw">The tournament draw</param>
    /// <param name="roundNumber">The round number (0 for first round)</param>
    private void CreateInitialRound(Draw draw, int roundNumber)
    {
        // Handle odd number of teams by adding bye
        //Who gets the bye ?
        List<Team> roundTeams = new List<Team>(_tournament.Teams);
        if (roundTeams.Count % 2 == 1)
        {
            roundTeams.Add(new Team(-1, "BYE"));
        }

        int noPermutations = roundTeams.Count / 2;

        // For the first round, pair top half vs bottom half
        //TODO: Configurable. Random, adjacent, etc.    
        for (int p = 0; p < noPermutations; p++)
        {
            Team home = roundTeams[p];
            Team away = roundTeams[roundTeams.Count - p - 1];

            Debug.WriteLine($"Round {roundNumber}: ({home.Index},{away.Index}) - {home.Label} vs {away.Label}");

            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, roundNumber);
                permutation.Id = p;
                draw.AddPermutation(permutation, roundNumber, $"Round {roundNumber + 1}");
            }
        }
    }

    /// <summary>
    /// Create the next round based on current standings and Swiss pairing rules.
    /// </summary>
    /// <param name="draw">The tournament draw</param>
    /// <param name="roundNumber">The current round number</param>
    /// <param name="scores">Scores from previous rounds</param>
    private void CreateNextRound(Draw draw, int roundNumber, List<Score> scores)
    {
        // Get current standings from the most recent completed round
        var standings = GetCurrentStandings();

        // Group teams by their current points (Swiss format rule: pair teams with similar scores)
        var scoreGroups = standings.GroupBy(s => s.Points)
                                  .OrderByDescending(g => g.Key)
                                  .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.Team.Ranking).ToList());

        Debug.WriteLine($"Score groups for Round {roundNumber + 1}:");
        foreach (var group in scoreGroups)
        {
            Debug.WriteLine($"  {group.Key} pts: {string.Join(", ", group.Value.Select(s => s.Team.Label))} ({group.Value.Count} teams)");
        }

        // Create pairings following Swiss format rules
        var pairings = CreateSwissStylePairings(scoreGroups, draw, roundNumber);

        // Create matches for the pairings
        for (int p = 0; p < pairings.Count; p++)
        {
            var (home, away) = pairings[p];

            Debug.WriteLine($"Round {roundNumber}: ({home.Index},{away.Index}) - {home.Label} vs {away.Label}");

            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, roundNumber);
                permutation.Id = p;
                draw.AddPermutation(permutation, roundNumber, $"Round {roundNumber + 1}");
            }
        }
    }

    /// <summary>
    /// Updates standings by adding results from the just-completed round to previous standings.
    /// </summary>
    /// <param name="draw">The tournament draw</param>
    /// <param name="completedRound">The round that was just completed</param>
    /// <param name="scores">All scores from the tournament</param>
    private void UpdateStandingsForCompletedRound(Draw draw, int completedRound, List<Score> scores)
    {
        // Get previous standings (or create initial if this is round 0)
        List<TeamStanding> currentStandings;
        if (completedRound == 0)
        {
            // Initialize standings for the first round
            currentStandings = _tournament.Teams.Select(team => new TeamStanding
            {
                Team = team,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                Points = 0
            }).ToList();
        }
        else
        {
            // Copy previous round's standings
            var previousStandings = GetStandingsForRound(completedRound - 1);
            if (previousStandings == null)
            {
                throw new InvalidOperationException($"Previous round standings not found for round {completedRound - 1}");
            }

            currentStandings = previousStandings.Select(s => new TeamStanding
            {
                Team = s.Team,
                Wins = s.Wins,
                Losses = s.Losses,
                Draws = s.Draws,
                Points = s.Points
            }).ToList();
        }

        // Process only the matches from the completed round
        var completedRoundScores = scores.Where(s => s.Round == completedRound);
        var matchScores = completedRoundScores.GroupBy(s => new { s.Permutation, s.Match });

        foreach (var matchGroup in matchScores)
        {
            var matchScoresList = matchGroup.ToList();
            if (matchScoresList.Any())
            {
                var winner = DetermineMatchWinner(matchScoresList);
                var loser = DetermineMatchLoser(matchScoresList);

                // Update standings with results from this match
                if (winner != null && loser != null)
                {
                    var winnerStanding = currentStandings.FirstOrDefault(s => s.Team.Id == winner.Id);
                    var loserStanding = currentStandings.FirstOrDefault(s => s.Team.Id == loser.Id);

                    if (winnerStanding != null)
                    {
                        winnerStanding.Wins++;
                        winnerStanding.Points += 1.0; // Winner gets 1 point
                    }
                    if (loserStanding != null) loserStanding.Losses++;
                }
                else if (winner == null && loser == null)
                {
                    // It's a draw - both teams get a draw
                    var round = draw.Rounds.FirstOrDefault(r => r.Index == completedRound);
                    var permutation = round?.Permutations.FirstOrDefault(p => p.Id == matchGroup.Key.Permutation);
                    var match = permutation?.Matches.FirstOrDefault(m => m.Id == matchGroup.Key.Match);

                    if (match != null)
                    {
                        var homeTeam = match.Home.FirstOrDefault()?.Team;
                        var awayTeam = match.Away.FirstOrDefault()?.Team;

                        if (homeTeam != null && awayTeam != null)
                        {
                            var homeStanding = currentStandings.FirstOrDefault(s => s.Team.Id == homeTeam.Id);
                            var awayStanding = currentStandings.FirstOrDefault(s => s.Team.Id == awayTeam.Id);

                            if (homeStanding != null)
                            {
                                homeStanding.Draws++;
                                homeStanding.Points += 0.5; // Draw gets 0.5 points
                            }
                            if (awayStanding != null)
                            {
                                awayStanding.Draws++;
                                awayStanding.Points += 0.5; // Draw gets 0.5 points
                            }
                        }
                    }
                }
            }
        }

        // Record the updated standings for this round
        RecordStandingsForRound(completedRound, currentStandings);
    }

    /// <summary>
    /// Create Swiss-style pairings following proper Swiss format rules.
    /// Groups teams by score, handles odd groups, and pairs within groups.
    /// </summary>
    /// <param name="scoreGroups">Teams grouped by their current points</param>
    /// <param name="draw">Current tournament draw</param>
    /// <param name="roundNumber">Current round number</param>
    /// <returns>List of team pairings</returns>
    private List<(Team Home, Team Away)> CreateSwissStylePairings(Dictionary<double, List<TeamStanding>> scoreGroups, Draw draw, int roundNumber)
    {
        var pairings = new List<(Team, Team)>();
        var processedGroups = new List<double>();

        // Make a working copy of score groups
        // Sort players into score groups (highest to lowest score).
        // key= 
        var copyOfScoreGroups = scoreGroups.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.Select(s => s.Team).ToList()
        );

        // Handle byes first - find if we have an odd total number of teams
        int totalTeams = copyOfScoreGroups.Values.Sum(g => g.Count);
        Team? byeTeam = null;

        if (totalTeams % 2 == 1)
        {
            byeTeam = AssignBye(copyOfScoreGroups, draw);
            if (byeTeam != null)
            {
                Debug.WriteLine($"Bye assigned to: {byeTeam.Label}");
            }
        }

        // Process each score group from highest to lowest
        foreach (var scoreGroup in copyOfScoreGroups.OrderByDescending(kvp => kvp.Key))
        {
            double points = scoreGroup.Key;
            List<Team> teams = scoreGroup.Value;

            if (teams.Count == 0) continue; // Skip empty groups

            Debug.WriteLine($"Processing score group {points} pts with {teams.Count} teams");

            // If odd number in group, borrow from next lower group
            if (teams.Count % 2 == 1)
            {
                //upfloaters
                var borrowedTeam = BorrowFromLowerGroup(copyOfScoreGroups, points, teams, draw);
                if (borrowedTeam != null)
                {
                    teams.Add(borrowedTeam);
                    Debug.WriteLine($"Borrowed {borrowedTeam.Label} from lower group");
                }
            }

            // Pair teams within this group
            var groupPairings = PairWithinGroup(teams, draw, roundNumber);
            pairings.AddRange(groupPairings);

            processedGroups.Add(points);
        }

        return pairings;
    }

    /// <summary>
    /// Assigns a bye to the most appropriate team following Swiss rules.
    /// Priority: single-player score groups (highest to lowest), then team that hasn't had a bye, then lowest ranked.
    /// </summary>
    /// <param name="scoreGroups">Teams grouped by their current points</param>
    /// <param name="draw">Tournament draw to check bye history</param>
    /// <returns>Team assigned the bye, or null if none needed</returns>
    private Team? AssignBye(Dictionary<double, List<Team>> scoreGroups, Draw draw)
    {

        // If no single-player group has a team without a bye, fall back to original logic
        // Start from lowest score group
        foreach (var group in scoreGroups.OrderBy(kvp => kvp.Key))
        {
            var teams = group.Value;
            if (teams.Count == 0) continue;

            // First try to find a team that hasn't had a bye
            var teamWithoutBye = teams.FirstOrDefault(t => !HasReceivedBye(t, draw));
            if (teamWithoutBye != null)
            {
                teams.Remove(teamWithoutBye);
                return teamWithoutBye;
            }

            // If all teams have had byes, take the lowest ranked
            var lowestRanked = teams.OrderBy(t => t.Ranking).First();
            teams.Remove(lowestRanked);
            return lowestRanked;
        }

        return null;
    }

    /// <summary>
    /// Borrows a team from the next lower score group to balance odd-sized groups.
    /// Prioritizes teams that haven't played against any member of the higher group.
    /// </summary>
    /// <param name="workingGroups">Current working groups</param>
    /// <param name="currentPoints">Points of the current group</param>
    /// <param name="higherGroupTeams">Teams in the higher point group</param>
    /// <param name="draw">Tournament draw to check match history</param>
    /// <returns>Team borrowed from lower group, or null</returns>
    private Team? BorrowFromLowerGroup(Dictionary<double, List<Team>> workingGroups, double currentPoints, List<Team> higherGroupTeams, Draw draw)
    {
        // Find the next lower score group that has teams
        var lowerGroups = workingGroups.Where(kvp => kvp.Key < currentPoints && kvp.Value.Count > 0)
                                      .OrderByDescending(kvp => kvp.Key);

        foreach (var lowerGroup in lowerGroups)
        {
            var teams = lowerGroup.Value;
            if (teams.Count > 1 || (teams.Count == 1 && workingGroups.Values.Sum(g => g.Count) % 2 == 0))
            {
                // First, try to find a team that hasn't played against any member of the higher group
                var candidatesWithoutRematches = teams
                    .Where(lowerTeam => !higherGroupTeams.Any(higherTeam => HaveTeamsPlayed(lowerTeam, higherTeam, draw)))
                    .OrderByDescending(t => t.Ranking)
                    .ToList();

                Team teamToBorrow;
                if (candidatesWithoutRematches.Any())
                {
                    // Prefer highest ranked team that hasn't played against higher group
                    teamToBorrow = candidatesWithoutRematches.First();
                }
                else
                {
                    // Fall back to highest ranked team if all have played against higher group
                    teamToBorrow = teams.OrderByDescending(t => t.Ranking).First();
                }

                teams.Remove(teamToBorrow);
                return teamToBorrow;
            }
        }

        return null;
    }

    /// <summary>
    /// Pairs teams within a score group, avoiding rematches when possible.
    /// </summary>
    /// <param name="teams">Teams in the same score group</param>
    /// <param name="draw">Tournament draw to check match history</param>
    /// <param name="roundNumber">Current round number</param>
    /// <returns>List of pairings within the group</returns>
    private List<(Team, Team)> PairWithinGroup(List<Team> teams, Draw draw, int roundNumber)
    {
        var pairings = new List<(Team, Team)>();
        var availableTeams = new List<Team>(teams);

        // Sort teams by ranking (highest first) for consistent pairing
        availableTeams.Sort((x, y) => y.Ranking.CompareTo(x.Ranking));

        while (availableTeams.Count >= 2)
        {
            var team1 = availableTeams[0];
            availableTeams.RemoveAt(0);

            Team? opponent = null;

            // Try to find opponent that hasn't played against team1
            for (int i = 0; i < availableTeams.Count; i++)
            {
                var candidate = availableTeams[i];
                if (!HaveTeamsPlayed(team1, candidate, draw))
                {
                    opponent = candidate;
                    availableTeams.RemoveAt(i);
                    break;
                }
            }

            // If no unplayed opponent found, take the next available (closest ranked)
            if (opponent == null && availableTeams.Count > 0)
            {
                opponent = availableTeams[0];
                availableTeams.RemoveAt(0);
            }

            if (opponent != null)
            {
                pairings.Add((team1, opponent));
                Debug.WriteLine($"  Paired: {team1.Label} vs {opponent.Label}");
            }
        }

        return pairings;
    }

    /// <summary>
    /// Check if two teams have already played against each other.
    /// </summary>
    /// <param name="team1">First team</param>
    /// <param name="team2">Second team</param>
    /// <param name="draw">Tournament draw to check</param>
    /// <returns>True if teams have played before</returns>
    private bool HaveTeamsPlayed(Team team1, Team team2, Draw draw)
    {
        foreach (var round in draw.Rounds)
        {
            foreach (var permutation in round.Permutations)
            {
                var teams = permutation.Teams.ToList();
                if (teams.Count >= 2)
                {
                    bool hasTeam1 = teams.Any(t => t.Id == team1.Id);
                    bool hasTeam2 = teams.Any(t => t.Id == team2.Id);

                    if (hasTeam1 && hasTeam2)
                        return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check if a team has received a bye in previous rounds.
    /// </summary>
    /// <param name="team">Team to check</param>
    /// <param name="draw">Tournament draw</param>
    /// <returns>True if team has received a bye</returns>
    private bool HasReceivedBye(Team team, Draw draw)
    {
        foreach (var round in draw.Rounds)
        {
            foreach (var permutation in round.Permutations)
            {
                var teams = permutation.Teams.ToList();
                if (teams.Any(t => t.Id == team.Id && teams.Any(bt => bt.Label == "BYE")))
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determine the winner of a match based on scores.
    /// </summary>
    /// <param name="matchScores">Scores for the match</param>
    /// <returns>Winning team or null</returns>
    private Team? DetermineMatchWinner(List<Score> matchScores)
    {
        if (!matchScores.Any()) return null;

        var firstScore = matchScores.First();

        // Find the actual match to get the teams
        var round = _tournament.Draw?.Rounds.FirstOrDefault(r => r.Index == firstScore.Round);
        var permutation = round?.Permutations.FirstOrDefault(p => p.Id == firstScore.Permutation);
        var match = permutation?.Matches.FirstOrDefault(m => m.Id == firstScore.Match);

        if (match == null) return null;

        // For tennis scoring, check sets won
        var homeTeam = match.Home.FirstOrDefault()?.Team;
        var awayTeam = match.Away.FirstOrDefault()?.Team;

        if (homeTeam == null || awayTeam == null) return null;

        // Group scores by set to determine set winners
        var setScores = matchScores.GroupBy(s => s.Set);
        int homeSetsWon = 0;
        int awaySetsWon = 0;

        foreach (var setGroup in setScores)
        {
            int homeSetScore = setGroup.Sum(s => s.Home);
            int awaySetScore = setGroup.Sum(s => s.Away);

            if (homeSetScore > awaySetScore)
                homeSetsWon++;
            else if (awaySetScore > homeSetScore)
                awaySetsWon++;
        }

        // Return winner based on sets won
        if (homeSetsWon > awaySetsWon)
            return homeTeam;
        else if (awaySetsWon > homeSetsWon)
            return awayTeam;

        // In case of tie, use total games
        int homeTotalGames = matchScores.Sum(s => s.Home);
        int awayTotalGames = matchScores.Sum(s => s.Away);

        if (homeTotalGames > awayTotalGames)
            return homeTeam;
        else if (awayTotalGames > homeTotalGames)
            return awayTeam;

        return null; // True draw
    }

    /// <summary>
    /// Determine the loser of a match based on scores.
    /// </summary>
    /// <param name="matchScores">Scores for the match</param>
    /// <returns>Losing team or null</returns>
    private Team? DetermineMatchLoser(List<Score> matchScores)
    {
        if (!matchScores.Any()) return null;

        var firstScore = matchScores.First();

        // Find the actual match to get the teams
        var round = _tournament.Draw?.Rounds.FirstOrDefault(r => r.Index == firstScore.Round);
        var permutation = round?.Permutations.FirstOrDefault(p => p.Id == firstScore.Permutation);
        var match = permutation?.Matches.FirstOrDefault(m => m.Id == firstScore.Match);

        if (match == null) return null;

        var homeTeam = match.Home.FirstOrDefault()?.Team;
        var awayTeam = match.Away.FirstOrDefault()?.Team;

        if (homeTeam == null || awayTeam == null) return null;

        // Group scores by set to determine set winners
        var setScores = matchScores.GroupBy(s => s.Set);
        int homeSetsWon = 0;
        int awaySetsWon = 0;

        foreach (var setGroup in setScores)
        {
            int homeSetScore = setGroup.Sum(s => s.Home);
            int awaySetScore = setGroup.Sum(s => s.Away);

            if (homeSetScore > awaySetScore)
                homeSetsWon++;
            else if (awaySetScore > homeSetScore)
                awaySetsWon++;
        }

        // Return loser based on sets won
        if (homeSetsWon < awaySetsWon)
            return homeTeam;
        else if (awaySetsWon < homeSetsWon)
            return awayTeam;

        // In case of tie, use total games
        int homeTotalGames = matchScores.Sum(s => s.Home);
        int awayTotalGames = matchScores.Sum(s => s.Away);

        if (homeTotalGames < awayTotalGames)
            return homeTeam;
        else if (awayTotalGames < homeTotalGames)
            return awayTeam;

        return null; // True draw
    }

    /// <summary>
    /// Records the standings for a specific round in the tournament history.
    /// </summary>
    /// <param name="roundNumber">The round number to record standings for</param>
    /// <param name="standings">The calculated standings for the round</param>
    private void RecordStandingsForRound(int roundNumber, List<TeamStanding> standings)
    {
        // Sort standings by points (descending), then by team ranking (descending)
        var sortedStandings = standings.OrderByDescending(s => s.Points)
                                      .ThenByDescending(s => s.Team.Ranking)
                                      .ToList();

        // Assign positions based on sorted order
        for (int i = 0; i < sortedStandings.Count; i++)
        {
            sortedStandings[i].Position = i + 1;
        }

        // Store a deep copy of the standings for this round
        var roundStandings = sortedStandings.Select(s => new TeamStanding
        {
            Team = s.Team,
            Wins = s.Wins,
            Losses = s.Losses,
            Draws = s.Draws,
            Points = s.Points,
            Position = s.Position
        }).ToList();

        _standingsHistory[roundNumber] = roundStandings;

        Debug.WriteLine($"Standings after Round {roundNumber + 1}:");
        foreach (var standing in roundStandings)
        {
            Debug.WriteLine($"  {standing.Position}. {standing.Team.Label} - W:{standing.Wins} L:{standing.Losses} D:{standing.Draws} Pts:{standing.Points}");
        }
    }

    /// <summary>
    /// Gets the standings for a specific round.
    /// </summary>
    /// <param name="roundNumber">The round number (0-based)</param>
    /// <returns>List of team standings for the round, or null if round not found</returns>
    public List<TeamStanding>? GetStandingsForRound(int roundNumber)
    {
        return _standingsHistory.TryGetValue(roundNumber, out var standings) ? standings : null;
    }

    /// <summary>
    /// Gets all historical standings data.
    /// </summary>
    /// <returns>Dictionary with round numbers as keys and standings as values</returns>
    public Dictionary<int, List<TeamStanding>> GetAllStandings()
    {
        return new Dictionary<int, List<TeamStanding>>(_standingsHistory);
    }

    /// <summary>
    /// Gets the current standings (most recent round).
    /// </summary>
    /// <returns>Current standings or empty list if no rounds completed</returns>
    public List<TeamStanding> GetCurrentStandings()
    {
        if (!_standingsHistory.Any()) return new List<TeamStanding>();

        var latestRound = _standingsHistory.Keys.Max();
        return _standingsHistory[latestRound];
    }

    /// <summary>
    /// Calculates the expected number of rounds for a Swiss tournament based on team count.
    /// </summary>
    /// <param name="teamCount">Number of teams in the tournament</param>
    /// <returns>Expected number of rounds</returns>
    public static int ExpectedNumberOfRounds(int teamCount)
    {
        if (teamCount <= 1) return 0;
        return (int)Math.Ceiling(Math.Log2(teamCount)) + 1;
    }
}