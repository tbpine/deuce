using System.Diagnostics;
using deuce.ext;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Serialization;

namespace deuce;

class DrawMakerBrackets : DrawMakerBase, IDrawMaker
{
    private readonly IGameMaker _gameMaker;

    public DrawMakerBrackets(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;

    }

    public Draw Create(List<Team> teams)
    {
        //Losers fall to a second bracket which
        //The result
        Draw schedule = new Draw(_tournament);

        //Assigns
        _teams = teams;

        //For knockout tournaments, make sure there's an even number of teams
        //by adding bye teams if necessary.
        int exponent = (int)Math.Ceiling(Math.Log2(_teams.Count));
        int noByes = (int)Math.Pow(2, exponent) - teams.Count;

        //If there are any byes needed, add them to the list of teams.
        for (int i = 0; i < noByes; i++)
        {
            var byeTeam = new Team() { Index = _teams.Count, Label = $"" };
            byeTeam.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, _teams.Count);
            _teams.Add(byeTeam);
        }

        var upperfactoryScheduler = new FactoryDrawMaker();
        var upperScheduler = upperfactoryScheduler.Create(_tournament, _gameMaker);
        upperScheduler.Create(_teams);

        //is a tournament in itself.
        //Losers bracket is equivalent to the second round of the main tournament.
        //The "NextRound" method will handle the logic for moving teams to the losers bracket.

        //Make a copy of the current tournament but 
        //change details and teams.
        Tournament losersBracket = (_tournament.Clone() as Tournament) ?? new();
        losersBracket.Details.TeamSize = _teams.Count / 2;
        //Make blank teams for the losers bracket
        for (int i = 0; i < losersBracket.Details.TeamSize; i++)
        {
            var byeTeam = new Team() { Index = i + 1 };
            byeTeam.CreateBye(losersBracket.Details.TeamSize, losersBracket.Organization, losersBracket.Teams.Count);
            losersBracket.Teams.Add(byeTeam);
        }

        var factoryScheduler = new FactoryDrawMaker();
        var loserBracketScheduler = factoryScheduler.Create(losersBracket, _gameMaker);
        loserBracketScheduler.Create(losersBracket.Teams);

        //link the losers bracket to the main tournament
        _tournament.AddBracket(new Bracket()
        {
            Upper = _tournament,
            Tournament = losersBracket
        });

        return schedule;
    }

    public void OnChange(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        Draw? winnersDraw = _tournament.Draw;
        Draw? losersDraw = _tournament.Brackets.FirstOrDefault()?.Tournament?.Draw;
        //Nothing to be done if no draws are available
        if (winnersDraw == null || losersDraw == null) return;

        //Use LINQ to group scores by match where the key is the match identifier.
        //This will allow us to process all scores for each match in one go.
        //This is useful for processing scores that may come from different sources
        //or for matches that have multiple scores (e.g., sets in tennis).
        //Group scores by match
        var groupedScores = from score in scores
                             group score by new { score.Match } into g
                             select new { MatchKey = g.Key, Scores = g.ToList() };

        //For all scores
        foreach (var sets in groupedScores)
        {
            var winnersMatch = winnersDraw.FindMatch(sets.MatchKey.Match);
            if (winnersMatch is not null)
            {
                var winner = DetermineMatchWinner(sets.Scores, winnersMatch);
                if (winner is null) continue;
                //Get all scores from the match
                AdvancePlayer(winnersDraw, sets.Scores, true, winner);

                var loser  = winnersMatch.GetLosingSide(winner);

                if (loser is null) continue;

                AdvancePlayer(losersDraw, sets.Scores, true, loser);
            }
            else
            {

                var losersWinningMatch = losersDraw.FindMatch(sets.MatchKey.Match);
                var loserBracketWinner = DetermineMatchWinner(sets.Scores, losersWinningMatch!);
                AdvancePlayer(losersDraw, sets.Scores, false, loserBracketWinner!);
            }

        }

    }

    private void AdvancePlayer(Draw draw, List<Score> scoreForMatch, bool winnerToHome, Team winner)
    {
        //Score from the losers bracket
        var match = draw.FindMatch(scoreForMatch.First());
        if (match?.Permutation is null) return;

        int nextRoundIndex = match.Round + 1;
        //Range check for next round
        if (nextRoundIndex < 1 || nextRoundIndex > draw.Rounds.Count) return;

        bool isOdd = (match.Permutation.Id % 2) > 0;
        int nextPermIdx = match.Permutation.Id - (isOdd ? 1 : 0) / 2;
        //Find the next match in the next round
        var nextRound = draw.Rounds.First(r => r.Index == nextRoundIndex);
        var nextMatch = nextRound.Permutations.FirstOrDefault(p => p.Id == nextPermIdx)?.Matches.First();
        match?.Permutation.AddTeam(winner);

        if (winnerToHome)
        {
            nextMatch?.ClearHomePlayers();
            foreach (Player player in winner.Players) nextMatch?.AddHome(player);
        }

        else
        {
            nextMatch?.ClearAwayPlayers();
            foreach (Player player in winner.Players) nextMatch?.AddAway(player);
        }

    }

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