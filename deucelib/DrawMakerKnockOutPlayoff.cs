using System.Diagnostics;
using deuce.ext;

namespace deuce;

/// <summary>
/// Implements a knockout tournament scheduler with playoff and loser bracket creation.
/// Similar to the knockout tournament structure, but for each round, creates 
/// a playoff round (with half the matches) and a loser round (with half the matches).
/// In this tournament structure, teams are eliminated after losing a single match,
/// with winners advancing to the next round until only one team remains, but
/// each round includes additional playoff and loser brackets for continued competition.
/// </summary>
/// <remarks>
/// The scheduler automatically adds "BYE" teams if the number of participating teams
/// is not a power of 2, ensuring proper bracket structure. Teams are ranked and
/// paired such that the highest-ranked team plays the lowest-ranked team in the first round.
/// Each round creates three brackets: main (full matches), playoff (half matches), and loser (half matches).
/// </remarks>
class DrawMakerKnockOutPlayoff : DrawMakerBase, IDrawMaker
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
    /// Initializes a new instance of the DrawMakerKnockOutPlayoff class.
    /// </summary>
    /// <param name="t">The tournament for which to create the knockout playoff schedule.</param>
    /// <param name="gameMaker">The game maker instance used to create matches.</param>
    public DrawMakerKnockOutPlayoff(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    /// <summary>
    /// Creates a complete knockout tournament schedule with playoff and loser bracket creation.
    /// For each round in the main tournament, creates a playoff round (half matches) and loser round (half matches).
    /// </summary>
    /// <param name="teams">The list of teams participating in the tournament.</param>
    /// <returns>A complete Draw object containing all rounds and matches for the main knockout tournament, playoff rounds, and loser rounds.</returns>
    /// <remarks>
    /// The method performs the following steps:
    /// 1. Adds BYE teams if needed to make the total number a power of 2
    /// 2. Sorts teams by ranking in descending order
    /// 3. Creates the first round by pairing highest vs lowest ranked teams
    /// 4. Creates subsequent rounds with placeholder teams that will be filled as winners advance
    /// 5. For each round, creates a playoff round with half the matches and a loser round with half the matches
    /// 6. Calculates appropriate round labels (Final, Semi Final, Quarter Final)
    /// </remarks>
    public Draw Create(List<Team> teams)
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

        // Create first round matches for main tournament
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

        // Create first round matches for playoff field (half the matches of main round)
        CreatePlayoffRound(draw, 1, noPermutations / 2, "Playoff");

        // Create first round matches for loser bracket (half the matches of main round)
        CreateLoserRound(draw, 1, noPermutations / 2, "Loser");

        // From round 2 to the final round
        // Create placeholder matches with empty teams that will be populated as winners advance
        for (int r = 2; r <= noRounds; r++)
        {
            // In a knockout tournament, each round has half the number of matches as the previous round.
            // For example, 8 teams = 4 matches in the first round, 2 matches in the second round, and 1 match in the final.
            Debug.Write($"Round {r}:");
            noPermutations = (int)(_teams.Count / Math.Pow(2, r));

            // Create main tournament round
            CreateMainTournamentRound(draw, r, noPermutations, noRounds);

            // Create playoff field round (half the matches of main round)
            CreatePlayoffRound(draw, r, noPermutations / 2, GetPlayoffRoundLabel(noRounds, r));

            // Create loser bracket round (half the matches of main round)
            CreateLoserRound(draw, r, noPermutations / 2, GetLoserRoundLabel(noRounds, r));

            Debug.Write($"\n");
        }

        return draw;
    }

    /// <summary>
    /// Creates a main tournament round with placeholder teams.
    /// </summary>
    /// <param name="draw">The tournament draw to add matches to.</param>
    /// <param name="roundNumber">The round number being created.</param>
    /// <param name="noPermutations">Number of matches/permutations in this round.</param>
    /// <param name="totalRounds">Total number of rounds in the tournament.</param>
    private void CreateMainTournamentRound(Draw draw, int roundNumber, int noPermutations, int totalRounds)
    {
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
                var permutation = _gameMaker.Create(_tournament, home, away, roundNumber);
                permutation.Id = p;
                // Add round labels for finals, semi-finals, etc.
                draw.AddPermutation(permutation, roundNumber, GetRoundLabel(totalRounds, roundNumber));
            }
        }
    }

    /// <summary>
    /// Creates a playoff round with half the matches of the main tournament round.
    /// </summary>
    /// <param name="draw">The tournament draw to add playoff matches to.</param>
    /// <param name="roundNumber">The round number being duplicated.</param>
    /// <param name="noPermutations">Number of matches/permutations in this playoff round (half of main round).</param>
    /// <param name="playoffLabel">Label for the playoff round.</param>
    private void CreatePlayoffRound(Draw draw, int roundNumber, int noPermutations, string playoffLabel)
    {
        // Create the playoff round with half the matches of the main round
        // but with different teams (could be losers from previous rounds or additional teams)

        // Find the corresponding main round
        var mainRound = draw.Rounds.FirstOrDefault(r => r.Index == roundNumber);
        if (mainRound == null) return;

        // Ensure we have at least one match in the playoff round
        if (noPermutations < 1) noPermutations = 1;

        // Create a separate playoff round
        mainRound.Playoff = new Round(roundNumber)
        {
            Tournament = _tournament
        };

        // Make a list of empty teams as placeholders for playoff participants
        List<Team> playoffTeams = new List<Team>();

        // Create empty placeholder teams for this playoff round
        for (int i = 0; i < noPermutations * 2; i++)
        {
            var playoffTeam = new Team(); // Offset index to avoid conflicts
            playoffTeam.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, i);
            playoffTeams.Add(playoffTeam);
        }

        // Create playoff matches for this round
        for (int p = 0; p < noPermutations; p++)
        {
            var home = playoffTeams[p];
            var away = playoffTeams[playoffTeams.Count - p - 1];
            Debug.Write($"Playoff ({home.Index},{away.Index})");

            // Schedule playoff matches (only for tennis tournaments)
            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, roundNumber);
                permutation.Id = p; // Changed from p + 1000 to p to start at 0
                // Link the permutation to the playoff round
                permutation.Round = mainRound.Playoff;
                mainRound.Playoff.AddPerm(permutation);
            }
        }

    }

    /// <summary>
    /// Creates a loser round with half the matches of the main tournament round.
    /// </summary>
    /// <param name="draw">The tournament draw to add loser matches to.</param>
    /// <param name="roundNumber">The round number being duplicated.</param>
    /// <param name="noPermutations">Number of matches/permutations in this loser round (half of main round).</param>
    /// <param name="loserLabel">Label for the loser round.</param>
    private void CreateLoserRound(Draw draw, int roundNumber, int noPermutations, string loserLabel)
    {
        // Create the loser round with half the matches of the main round
        // but with different teams (losers from previous rounds)

        // Find the corresponding main round
        var mainRound = draw.Rounds.FirstOrDefault(r => r.Index == roundNumber);
        if (mainRound == null) return;

        // Ensure we have at least one match in the loser round
        if (noPermutations < 1) noPermutations = 1;

        // Create a separate loser round
        mainRound.Loser = new Round(roundNumber)
        {
            Tournament = _tournament
        };

        // Make a list of empty teams as placeholders for loser participants
        List<Team> loserTeams = new List<Team>();

        // Create empty placeholder teams for this loser round
        for (int i = 0; i < noPermutations * 2; i++)
        {
            var loserTeam = new Team(); // Offset index to avoid conflicts
            loserTeam.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, i + 2000); // Offset by 2000 to avoid conflicts
            loserTeams.Add(loserTeam);
        }

        // Create loser matches for this round
        for (int p = 0; p < noPermutations; p++)
        {
            var home = loserTeams[p];
            var away = loserTeams[loserTeams.Count - p - 1];
            Debug.Write($"Loser ({home.Index},{away.Index})");

            // Schedule loser matches (only for tennis tournaments)
            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, roundNumber);
                permutation.Id = p; // Start at 0 for loser round
                // Link the permutation to the loser round
                permutation.Round = mainRound.Loser;
                mainRound.Loser.AddPerm(permutation);
            }
        }

    }

    /// <summary>
    /// Gets playoff-specific round labels.
    /// </summary>
    /// <param name="totalRounds">The total number of rounds in the tournament.</param>
    /// <param name="currentRound">The current round number for which to get the playoff label.</param>
    /// <returns>A string label for the playoff round.</returns>
    private string GetPlayoffRoundLabel(int totalRounds, int currentRound)
    {
        string baseLabel = GetRoundLabel(totalRounds, currentRound);
        return !string.IsNullOrEmpty(baseLabel) ? $"Playoff {baseLabel}" : "Playoff";
    }

    /// <summary>
    /// Gets loser-specific round labels.
    /// </summary>
    /// <param name="totalRounds">The total number of rounds in the tournament.</param>
    /// <param name="currentRound">The current round number for which to get the loser label.</param>
    /// <returns>A string label for the loser round.</returns>
    private string GetLoserRoundLabel(int totalRounds, int currentRound)
    {
        string baseLabel = GetRoundLabel(totalRounds, currentRound);
        return !string.IsNullOrEmpty(baseLabel) ? $"Loser {baseLabel}" : "Loser";
    }

    /// <summary>
    /// Specific to knockout tournaments. Get Final, Quarters, semis....
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

    public void OnChange(Draw draw, int round, int previousRound, List<Score> scores)
    {
        //for all scores
        foreach (var score in scores)
        {
            Round? mainRound = draw.Rounds.FirstOrDefault(r => r.Index == round);
            Round? playoffRound = mainRound?.Playoff;
            Round? loserRound = mainRound?.Loser;
            
            // Process each score
            //Find the match and round the score belongs to
            var matchRound = mainRound?.FindMatch(score.Match) ?? new(null, null);
            Match? match = matchRound.Item1;
            Round? roundForMatch = matchRound.Item2;

            //Continue if either values is null
            if (match == null || roundForMatch == null) continue;
            
            //Check which round it is
            bool isPlayoff = roundForMatch?.Equals(playoffRound) ?? false;
            bool isLoser = roundForMatch?.Equals(loserRound) ?? false;
            
            //winners in the mainRound progress to the next main round
            var winner = DetermineMatchWinner(scores, match);
            var loser = match.GetLosingSide(winner);

            if ((round + 1) > draw.Rounds.Count()) continue;

            var nextRound = isPlayoff ? null : draw.Rounds.FirstOrDefault(r => r.Index == round + 1);
            nextRound ??= draw.Rounds.FirstOrDefault(r => r.Index == round + 1 && r.Playoff != null)?.Playoff;
            
            // 0 goes to 0 in the next round, 1 goes to 0 in the next round, 2 goes to 1 in the next round
            // 3 goes to 1 in the next round
            bool isOdd = (match.Permutation?.Id ?? 0) % 2 > 0;
            //Permutation starts Id (it's meant to be index ) starts at 1
            int nextPermutationIndex = isOdd ? ((match.Permutation?.Id ?? 0) - 1) / 2  : (match.Permutation?.Id ?? 0) / 2;
            //For the first match
            nextPermutationIndex  = nextPermutationIndex < 0 ? 0 : nextPermutationIndex;
            var winnersMatch = nextRound?.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex)?.Matches.FirstOrDefault();

            //winners goto the next round
            if (isPlayoff) winnersMatch?.SetAwaySide(winner);
            else if (isLoser)
            {
                // Winners from loser round stay in loser bracket
                if (isOdd) winnersMatch?.SetHomeSide(winner);
                else winnersMatch?.SetAwaySide(winner);
            }
            else
            {
                // Main round winners advance to next main round
                if (isOdd) winnersMatch?.SetHomeSide(winner);
                else winnersMatch?.SetAwaySide(winner);
            }   

            if (round == 1 && !isPlayoff && !isLoser)
            {
                //special case for the first round, all losers from the main round
                //goto the playoff round
                var losingMatch = playoffRound?.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex)?.Matches.FirstOrDefault();
                if (isOdd) losingMatch?.SetHomeSide(isOdd ? loser : null);
                else losingMatch?.SetAwaySide(isOdd ? null : loser);
                
                // Also send losers to loser bracket
                var loserMatch = loserRound?.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex / 2)?.Matches.FirstOrDefault();
                if (nextPermutationIndex % 2 == 0) loserMatch?.SetHomeSide(loser);
                else loserMatch?.SetAwaySide(loser);
            }
            else if (!isPlayoff && !isLoser)
            {
                // For subsequent rounds, losers from main rounds go to loser bracket
                var loserMatch = loserRound?.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex / 2)?.Matches.FirstOrDefault();
                if (nextPermutationIndex % 2 == 0) loserMatch?.SetHomeSide(loser);
                else loserMatch?.SetAwaySide(loser);
            }

        }
    }


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
