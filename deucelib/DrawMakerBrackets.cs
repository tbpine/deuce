using System.Diagnostics;
using System.Runtime.Versioning;
using deuce.ext;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json.Serialization;

namespace deuce;

/// <summary>
/// Creates and manages double-elimination tournament brackets with both winners and losers brackets.
/// This class implements a double-elimination tournament structure where teams that lose a match
/// in the winners bracket fall to a losers bracket, giving them a second chance to compete.
/// </summary>
/// <remarks>
/// The DrawMakerBrackets class handles the creation of complex tournament structures including:
/// - Winners bracket (main tournament bracket)
/// - Losers bracket (secondary bracket for eliminated teams)
/// - Automatic bye team generation to ensure proper bracket sizing
/// - Score processing and team advancement logic
/// - Route finding for determining next match destinations
/// 
/// The tournament follows standard double-elimination rules where teams must lose twice
/// to be eliminated from the tournament completely.
/// </remarks>
class DrawMakerBrackets : DrawMakerBase
{
    /// <summary>
    /// The game maker responsible for creating individual matches and games within the tournament.
    /// </summary>
    private readonly IGameMaker _gameMaker;

    /// <summary>
    /// Route finder used to determine the next match destination for advancing teams.
    /// The route finder is created based on the tournament type and handles navigation
    /// through the bracket structure.
    /// </summary>
    private IRouteFinder? _routeFinder;

    /// <summary>
    /// Initializes a new instance of the DrawMakerBrackets class for creating double-elimination tournaments.
    /// </summary>
    /// <param name="t">The tournament configuration that defines the structure and rules for the bracket.</param>
    /// <param name="gameMaker">The game maker responsible for creating individual matches within the tournament.</param>
    /// <remarks>
    /// The constructor automatically creates a route finder based on the tournament type,
    /// which will be used to determine match progression paths through the bracket structure.
    /// </remarks>
    public DrawMakerBrackets(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
        //Make a route finder based on the tournament type
        IRouteFinderFactory routeFinderFactory = new RouteFinderFactory();
        _routeFinder = routeFinderFactory.Create(t.Type);

    }

    /// <summary>
    /// Creates a complete double-elimination tournament draw with both winners and losers brackets.
    /// </summary>
    /// <param name="teams">The list of teams participating in the tournament.</param>
    /// <returns>A Draw object representing the complete tournament structure with both brackets.</returns>
    /// <remarks>
    /// This method performs the following operations:
    /// 1. Calculates the number of bye teams needed to create a power-of-2 tournament size
    /// 2. Adds bye teams as necessary to ensure proper bracket structure
    /// 3. Creates the main winners bracket using the provided teams
    /// 4. Creates a losers bracket with half the capacity of the winners bracket
    /// 5. Links both brackets together in a double-elimination structure
    /// 
    /// The losers bracket is initialized with bye teams that will be replaced as teams
    /// are eliminated from the winners bracket during tournament play.
    /// </remarks>
    public override Draw Create(List<Team> teams)
    {
        //Losers fall to a second bracket which
        //The result
        Draw draw = new Draw(_tournament);

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

        var winnersFactoryDrawMaker = new FactoryDrawMaker();
        //Create a KO tournament type draw maker
        var winnersDrawMaker = winnersFactoryDrawMaker.Create(new TournamentType(2, "","", "", ""), _tournament, _gameMaker);
        draw = winnersDrawMaker.Create(_teams);

        //is a tournament in itself.
        //Losers bracket is equivalent to the second round of the main tournament.
        //The "NextRound" method will handle the logic for moving teams to the losers bracket.

        //Make a copy of the current tournament but 
        //change details and teams.
        Tournament losersBracket = (_tournament.Clone() as Tournament) ?? new();
        
        //Make blank teams for the losers bracket
        for (int i = 0; i < _teams.Count/2; i++)
        {
            var byeTeam = new Team() { Index = i + 1 };
            byeTeam.CreateBye(losersBracket.Details.TeamSize, losersBracket.Organization, losersBracket.Teams.Count + 1);
            losersBracket.Teams.Add(byeTeam);
        }

        var losersFactoryDrawMaker = new FactoryDrawMaker();
        var losersDrawMaker = losersFactoryDrawMaker.Create(new TournamentType(2, "","", "", ""), losersBracket, _gameMaker);
        losersBracket.Draw = losersDrawMaker.Create(losersBracket.Teams);

        //link the losers bracket to the main tournament
        _tournament.AddBracket(new Bracket()
        {
            Upper = _tournament,
            Tournament = losersBracket
        });

        return draw;
    }

    /// <summary>
    /// Processes match score updates and advances teams through the double-elimination bracket structure.
    /// </summary>
    /// <param name="schedule">The current tournament draw containing all match information.</param>
    /// <param name="round">The current round being processed.</param>
    /// <param name="previousRound">The previous round that was completed.</param>
    /// <param name="scores">A list of scores representing completed matches that need to be processed.</param>
    /// <remarks>
    /// This method handles the complex logic of double-elimination tournaments:
    /// 1. Groups scores by match to handle multiple scores per match (e.g., sets in tennis)
    /// 2. Determines match winners using sport-specific scoring rules
    /// 3. Advances winners in their respective brackets (winners or losers)
    /// 4. Moves losers from the winners bracket to the losers bracket
    /// 5. Advances winners within the losers bracket
    /// 
    /// The method processes both winners bracket matches (where losers fall to the losers bracket)
    /// and losers bracket matches (where winners continue in the losers bracket).
    /// </remarks>
    public override void OnChange(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        Draw? winnersDraw = _tournament.Draw;
        Draw? losersDraw = _tournament.Brackets?.FirstOrDefault()?.Tournament?.Draw;
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
            //Is this set of scores from the winners or losers bracket?

            var winnersMatch = winnersDraw.FindMatch(sets.MatchKey.Match);
            if (winnersMatch is not null)
            {
                var winner = DetermineMatchWinner(sets.Scores, winnersMatch);
                if (winner is null) continue;
                //Get all scores from the match
                AdvancePlayer(winnersDraw,  true, winner, winnersMatch);

                var loser  = winnersMatch.GetLosingSide(winner);

                if (loser is null) continue;
                //Careful here, the "winnersMatch" is in the winners draw
                //but they share the same permutation index.
                //Put the loser in the same round in the losers draw
                AdvancePlayer(losersDraw,  winnersMatch.Id % 2 > 0, loser, winnersMatch, false);
            }
            else
            {

                var losersWinningMatch = losersDraw.FindMatch(sets.MatchKey.Match);

                if (losersWinningMatch is null) continue;

                var loserBracketWinner = DetermineMatchWinner(sets.Scores, losersWinningMatch);

                if (loserBracketWinner is null) continue;
                
                AdvancePlayer(losersDraw, false, loserBracketWinner, losersWinningMatch);
            }

        }

    }    /// <summary>
    /// Advances a winning team to their next match in the tournament bracket.
    /// </summary>
    /// <param name="draw">The tournament draw containing the bracket structure to navigate.</param>
    /// <param name="winnerToHome">If true, places the winning team as the home team in the next match; 
    /// if false, places them as the away team.</param>
    /// <param name="winner">The team that won the match and needs to be advanced.</param>
    /// <param name="match">The completed match from which the team is advancing.</param>
    /// <param name="advanceRound">If true, advances to the next round when finding the destination match;
    /// <remarks>
    /// This method uses the route finder to determine the destination match for the winning team.
    /// The method handles team placement by either clearing and setting home players or 
    /// clearing and setting away players in the destination match, depending on the 
    /// winnerToHome parameter. All players from the winning team are transferred to 
    /// maintain team composition in the next round.
    /// </remarks>
    private void AdvancePlayer(Draw draw,
                               bool winnerToHome,
                               Team winner,
                               Match match,
                               bool advanceRound = true)
    {
        //use the route finder to find the next match
        var nextMatch = _routeFinder?.FindDestMatch(draw, match, advanceRound);

        if (nextMatch is null) return;
        

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
    
    /// <summary>
    /// Determines the winner of a tennis match based on sets won and games won as a tiebreaker.
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