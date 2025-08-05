using System.Diagnostics;
using deuce.ext;
using Newtonsoft.Json.Serialization;

namespace deuce;

class SchedulerBrackets : SchedulerBase, IScheduler
{
    private readonly IGameMaker _gameMaker;

    public SchedulerBrackets(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;

    }

    public Schedule Run(List<Team> teams)
    {
        //Losers fall to a second bracket which
        //The result
        Schedule schedule = new Schedule(_tournament);

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

        var upperfactoryScheduler = new FactorySchedulers();
        var upperScheduler = upperfactoryScheduler.Create(_tournament, _gameMaker);
        upperScheduler.Run(_teams);

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

        var factoryScheduler = new FactorySchedulers();
        var loserBracketScheduler = factoryScheduler.Create(losersBracket, _gameMaker);
        loserBracketScheduler.Run(losersBracket.Teams);

        //link the losers bracket to the main tournament
        _tournament.AddBracket(new Bracket()
        {
            Upper = _tournament,
            Tournament = losersBracket
        });

        return schedule;
    }



    public void BeforeEndRound(Schedule schedule, int round, List<Score> scores)
    {

    }


    public void NextRound(Schedule schedule, int round, int previousRound, List<Score> scores)
    {
        //IN the schedule , get the previous round permutations
        var previousPermutations = schedule.Rounds.FirstOrDefault(r => r.Index == previousRound)?.Permutations;
        if (previousPermutations == null)
        {
            Debug.WriteLine($"No previous permutations found for round {previousRound}");
            return;
        }
        //Store list of winners and losers
        List<Team> _winners = new();
        List<Team> _losers = new();

        //Each permutation has exactly one match for knockout tournaments.
        foreach (var permutation in previousPermutations)
        {
            var match = permutation.Matches.FirstOrDefault();
            if (match == null)
            {
                Debug.WriteLine($"No match found for permutation {permutation.Id} in round {previousRound}");
                continue;
            }

            //Determine the winner of the match
            var winner = DetermineMatchWinner(scores, match);
            if (winner is not null)
            {
                //Store the winner and loser

                _winners.Add(winner);
                var losingTeam = match.GetLosingSide(winner);
                if (losingTeam != null) _losers.Add(losingTeam);
                //0,2,4,6 , even permutations goto the home side of the next round match
                //1,3,5,7 , odd permutations goto the away side of the next round match
                int nextPermIndex = permutation.Id % 2 == 0 ? permutation.Id / 2 : (permutation.Id - 1) / 2;
                var nextRoundMatch = schedule.Rounds.FirstOrDefault(r => r.Index == round)?.Permutations[nextPermIndex].Matches.FirstOrDefault();
                if (nextRoundMatch != null)
                {
                    nextRoundMatch.ClearHomePlayers();
                    permutation.AddTeam(winner);
                    foreach (Player winningPlayer in winner.Players) nextRoundMatch.AddHome(winningPlayer);
                }
                else
                {
                    //Creat  a match for this permutation
                    var nextPerm = _gameMaker.Create(_tournament, winner, new Team(), round);
                    nextPerm.Id = nextPermIndex;
                    //Add it to the round
                    schedule.Rounds.FirstOrDefault(r => r.Index == round)?.AddPerm(nextPerm);
                }

            }


        }

        var losersTournament = _tournament.Brackets.FirstOrDefault()?.Tournament;
        //Get winners from the previous round
        if (losersTournament != null)
        {
            //Get the previous round
            var losersPreviousRound = losersTournament.Schedule?.Rounds.FirstOrDefault(r => r.Index == previousRound);
            //if there's no previous round, then 
            if (losersPreviousRound != null)
            {
                //Get the permutations from the previous round
                var losersPermutations = losersPreviousRound.Permutations;
                //For each permutation, add the losing team to the next round
                foreach (var permutation in losersPermutations)
                {
                    var match = permutation.Matches.FirstOrDefault();
                    if (match == null) continue;
                    var winner = DetermineMatchWinner(scores, match);
                    //
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