using System.Diagnostics;
using deuce.ext;
using DocumentFormat.OpenXml.Wordprocessing;

namespace deuce;

class DrawMakerKnockOutPlayoff : DrawMakerBase
{
    private readonly IGameMaker _gameMaker;

    //Keep a record of the last round winners and losers
    private List<Team> _winners = new();
    private List<Team> _losers = new();
    public List<Team> Winners => _winners;
    public List<Team> Losers => _losers;

    public DrawMakerKnockOutPlayoff(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    public override Draw Create()
    {
        //The result
        Draw draw = new Draw(_tournament);

        var teams = _tournament.Teams;

        int exponent = (int)Math.Ceiling(Math.Log2(teams.Count));
        int noByes = (int)Math.Pow(2, exponent) - teams.Count;

        for (int i = 0; i < noByes; i++)
        {
            Team emptyTeam = new Team();
            emptyTeam.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, _teams.Count + i);
            _teams.Add(emptyTeam);
        }

        teams.Sort((x, y) => (int)(y.Ranking - x.Ranking));
        for (int i = 0; i < teams.Count; i++) teams[i].Index = i + 1;
        //The extra round is beteween winners of the main round and playoff round
        int noRounds = (int)Math.Log2(teams.Count) + 1;

        int noPermutations = teams.Count / 2;

        Debug.WriteLine($"ex:{exponent}|byes:{noByes}|teams:{teams.Count}|r:{noRounds}|perms:{noPermutations}");

        for (int i = 0; i < noPermutations; i++)
        {
            var home = _teams[i];
            var away = _teams[_teams.Count - i - 1];

            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, 1);
                permutation.Id = i;
                draw.AddPermutation(permutation, 1);
            }
        }

        CreatePlayoffRound(draw, 1, noPermutations/2, "Playoff");

        for (int r = 2; r <= noRounds; r++)
        {
            Debug.Write($"Round {r}:");
            noPermutations = (int)(_teams.Count / Math.Pow(2, r));

            if (r == noRounds) noPermutations = 1; // Last round always has one match

            CreateMainTournamentRound(draw, r, noPermutations, noRounds);

            if (r < noRounds)
                CreatePlayoffRound(draw, r, noPermutations, GetPlayoffRoundLabel(noRounds, r));

            Debug.Write($"\n");
        }

        return draw;
    }

    private void CreateMainTournamentRound(Draw draw, int roundNumber, int noPermutations, int totalRounds)
    {

        // Create matches for this round using placeholder teams
        for (int p = 0; p < noPermutations; p++)
        {
            var home = new Team(); 
            home.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, p);
            var away = new Team();
            away.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, p + 1);
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

    private string GetPlayoffRoundLabel(int totalRounds, int currentRound)
    {
        string baseLabel = GetRoundLabel(totalRounds, currentRound);
        return !string.IsNullOrEmpty(baseLabel) ? $"Playoff {baseLabel}" : "Playoff";
    }

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

    public override void OnChange(Draw draw, int round, int previousRound, List<Score> scores)
    {
        //for all scores
        foreach (var score in scores)
        {
            //find the match
            Round? mainRound = draw.Rounds.FirstOrDefault(r => r.Index == round);
            Round? playoffRound = mainRound?.Playoff;

            var matchRound = mainRound?.FindMatch(score.Match) ?? new(null, null);

            // Process each score
            //Find the match and round the score belongs to

            Match? match = matchRound.Item1;
            Round? roundForMatch = matchRound.Item2;

            //Continue if either values is null
            if (match == null || roundForMatch == null || mainRound is null )
                continue;

            if (round < draw.Rounds.Count() && playoffRound is null) continue;
            
            //Check which round it is
                bool isPlayoff = roundForMatch?.Equals(playoffRound) ?? false;

            //winners in the mainRound progress to the next main round
            //Get a list of set scores for the match
            var listOfSetScores = scores.Where(s => s.Match == match.Id).ToList();

            var winner = DetermineMatchWinner(listOfSetScores, match);
            var loser = match.GetLosingSide(winner);


            // Route score progression based on round type using ProgressRoute objects
            if (isPlayoff)
                ProgressPlayoffRoundScore(draw, mainRound, match, winner);
            else
                ProgressMainRoundScore(draw, mainRound, match, winner, loser);
        }
    }


    /// <summary>
    /// Progresses scores for matches in the main round.
    /// Winners advance to the next main round, while losers go to the loser round.
    /// </summary>
    /// <param name="draw">The tournament draw.</param>
    /// <param name="round">The current round number.</param>
    /// <param name="match">The match that was completed.</param>
    /// <param name="winner">The winning team.</param>
    /// <param name="loser">The losing team.</param>
    private void ProgressMainRoundScore(Draw draw, Round round, Match match, Team? winner, Team? loser)
    {
        //Get the next Round
        Round? nextRound = draw.Rounds.FirstOrDefault(r => r.Index == round.Index + 1);
        //return if the next round is missing
        if (nextRound == null) return;


        // Calculate progression logic
        bool isOdd = (match.Permutation?.Id ?? 0) % 2 > 0;
        int nextPermutationIndex = CalculateNextPermutationIndex(match, isOdd);
        var nextMatch = nextRound.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex)?.Matches.FirstOrDefault();

        //Find the match in the next round

        // Winners advance to next main round using the ProgressRoute
        if (nextMatch is null) return;

        if (isOdd) nextMatch.SetAwaySide(winner);
        else nextMatch.SetHomeSide(winner);

        //Losers goto the losers round for the first round. Else, they goto the playoff
        //round
        int nextPermIdxPlayoff = round.Index == 1 ?  nextPermutationIndex:  match.Permutation?.Id ?? 0;
        SendPlayerToMatch(draw, round.Playoff ?? throw new InvalidOperationException("Missing playoff round"),
        nextPermIdxPlayoff, loser, round.Index == 1 ? isOdd : false);


    }

    private void ProgressPlayoffRoundScore(Draw draw, Round round, Match match, Team? winner)
    {
        //Winners progress to the playoff round in the next round of the main draw
        //Get the next round
        var nextRound = draw.Rounds.FirstOrDefault(r => r.Index == round.Index + 1)?.Playoff;
        //If the next round is missing, return
        if (nextRound == null)
        {
            nextRound = draw.Rounds.FirstOrDefault(r => r.Index == round.Index + 1);
        }
        
        //Go straight across
        int nextPermutationIndex = match.Permutation?.Id ?? 0;
        //Find the next match in the losers round of the next round
        var losersMatch = nextRound?.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex)?.Matches.FirstOrDefault();
        if (losersMatch != null) losersMatch.SetAwaySide(winner);

    }

   

    /// <summary>
    /// Calculates the next permutation index for team progression.
    /// </summary>
    /// <param name="match">The current match.</param>
    /// <param name="isOdd">Whether the current permutation ID is odd.</param>
    /// <returns>The index for the next round's permutation.</returns>
    private int CalculateNextPermutationIndex(Match match, bool isOdd)
    {
        int nextPermutationIndex = isOdd ? ((match.Permutation?.Id ?? 0) - 1) / 2 : (match.Permutation?.Id ?? 0) / 2;
        return nextPermutationIndex < 0 ? 0 : nextPermutationIndex;
    }



    /// <summary>
    /// Send a player to a match.
    /// </summary>
    /// <param name="draw">The tournament draw.</param>
    /// <param name="round">The destination round.</param>
    /// <param name="nextPermutationIndex">The permutation index for placement.</param>
    /// <param name="player">The player to send.</param>
    /// <param name="isAwaySide">Whether the player is on the away side.</param>
    private void SendPlayerToMatch(Draw draw, Round round, int nextPermutationIndex, Team? player, bool isAwaySide)
    {

        //Losers from the main round goes to the home side in the loser round        
        var theMatch = round.Permutations.FirstOrDefault(p => p.Id == nextPermutationIndex)?.Matches.FirstOrDefault();
        if (isAwaySide) theMatch?.SetAwaySide(player);
        else theMatch?.SetHomeSide(player);
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
