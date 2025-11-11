using System.Diagnostics;
using deuce.ext;

namespace deuce;

class DrawMakerSwiss : DrawMakerBase, IDrawMaker
{
    private readonly IGameMaker _gameMaker;
    public DrawMakerSwiss(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    public override Draw Create(List<Team> teams)
    {
        //The result
        Draw draw = new Draw(_tournament);
        //Assigns
        _teams = teams;

        // Sort teams by ranking initially (higher ranked teams first)
        _teams.Sort((x, y) => (int)(y.Ranking - x.Ranking));

        // Add indexes to teams
        for (int i = 0; i < _teams.Count; i++) 
            _teams[i].Index = i + 1;

        // Calculate number of rounds based on tournament format or default
        // Swiss format typically uses log2(n) rounds, but can be customized
        int noRounds = CalculateNumberOfRounds(_teams.Count);

        Debug.WriteLine($"Swiss Tournament: Teams:{_teams.Count}, Rounds:{noRounds}");

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
        // If this is not the first round, create the next round based on current standings
        if (round > 0 && round < CalculateNumberOfRounds(_teams.Count))
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
        int standardRounds = (int)Math.Ceiling(Math.Log2(teamCount));
        
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
        List<Team> roundTeams = new List<Team>(_teams);
        if (roundTeams.Count % 2 == 1)
        {
            roundTeams.Add(new Team(-1, "BYE"));
        }

        int noPermutations = roundTeams.Count / 2;

        // For the first round, pair top half vs bottom half
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
        // Calculate current standings based on scores
        var standings = CalculateStandings(scores);
        
        // Sort teams by current standing (wins, then ranking)
        var sortedTeams = standings.OrderByDescending(s => s.Wins)
                                 .ThenByDescending(s => s.Team.Ranking)
                                 .Select(s => s.Team)
                                 .ToList();

        // Handle odd number of teams
        if (sortedTeams.Count % 2 == 1)
        {
            // Give bye to the lowest ranked team that hasn't had a bye yet
            var byeCandidate = sortedTeams.LastOrDefault(t => !HasReceivedBye(t, draw));
            if (byeCandidate == null)
                byeCandidate = sortedTeams.Last();
            
            sortedTeams.Remove(byeCandidate);
        }

        // Pair teams using Swiss pairing algorithm
        var pairings = CreateSwissPairings(sortedTeams, draw, roundNumber);

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
    /// Calculate current standings based on match results.
    /// </summary>
    /// <param name="scores">All scores from previous rounds</param>
    /// <returns>List of team standings</returns>
    private List<TeamStanding> CalculateStandings(List<Score> scores)
    {
        var standings = _teams.Select(team => new TeamStanding 
        { 
            Team = team, 
            Wins = 0, 
            Losses = 0 
        }).ToList();

        // Group scores by match and determine winners
        var matchScores = scores.GroupBy(s => new { s.Round, s.Permutation, s.Match });

        foreach (var matchGroup in matchScores)
        {
            var matchScoresList = matchGroup.ToList();
            if (matchScoresList.Any())
            {
                var winner = DetermineMatchWinner(matchScoresList);
                var loser = DetermineMatchLoser(matchScoresList);

                if (winner != null)
                {
                    var winnerStanding = standings.FirstOrDefault(s => s.Team.Id == winner.Id);
                    if (winnerStanding != null) winnerStanding.Wins++;
                }

                if (loser != null)
                {
                    var loserStanding = standings.FirstOrDefault(s => s.Team.Id == loser.Id);
                    if (loserStanding != null) loserStanding.Losses++;
                }
            }
        }

        return standings;
    }

    /// <summary>
    /// Create Swiss-style pairings avoiding repeat matches when possible.
    /// </summary>
    /// <param name="teams">Teams sorted by current standing</param>
    /// <param name="draw">Current tournament draw</param>
    /// <param name="roundNumber">Current round number</param>
    /// <returns>List of team pairings</returns>
    private List<(Team Home, Team Away)> CreateSwissPairings(List<Team> teams, Draw draw, int roundNumber)
    {
        var pairings = new List<(Team, Team)>();
        var availableTeams = new List<Team>(teams);

        while (availableTeams.Count >= 2)
        {
            var team1 = availableTeams[0];
            availableTeams.RemoveAt(0);

            // Find the best opponent for team1 (closest in ranking that hasn't played yet)
            Team? opponent = null;
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

            // If no unplayed opponent found, take the next available team
            if (opponent == null && availableTeams.Count > 0)
            {
                opponent = availableTeams[0];
                availableTeams.RemoveAt(0);
            }

            if (opponent != null)
            {
                pairings.Add((team1, opponent));
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
    /// Represents a team's current standing in the tournament.
    /// </summary>
    private class TeamStanding
    {
        public Team Team { get; set; } = new Team();
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
    }

}