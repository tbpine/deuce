using System.Diagnostics;
using System.Threading.Tasks;
using deuce.ext;

namespace deuce;

/// <summary>
/// Teams are divided into groups of a specified size.
/// Each group has a main round and knockout rounds within the group.
/// Winner of the main round and knockout round advance.
/// </summary>
class DrawMakerGroups : DrawMakerBase
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
    private IOrganizerGroup _organizerGroup;

    private IDrawMaker _drawMakerKOPlayoff;
    //For the main draw (after groups)
    private IDrawMaker _drawMakerKO;


    public DrawMakerGroups(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
        _organizerGroup = new OrganizerGroupDefault();
        _drawMakerKOPlayoff = new DrawMakerKnockOutPlayoff(t, gameMaker);
        _drawMakerKO = new DrawMakerKnockOut(t, gameMaker);
    }

    public override Draw Create(List<Team> teams)
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

        //Create groups
        _organizerGroup.Assign(_tournament, _teams);
        //Create draws for each round
        IDrawMaker drawMakerKOPlayoff = new DrawMakerKnockOutPlayoff(_tournament, _gameMaker);
        foreach (Group group in _tournament.Groups)
        {
            //Create a draw for this group
            group.CreateDraw(drawMakerKOPlayoff);
        }

        //The last winning teams of main and playoff rounds
        //goes in the main tournament playoffs

        int noOfTeams = 2 * _tournament.Groups.Count();
        //Continue on with the main tournament 

        //Round is not group size
        int noRounds = (int)Math.Log2(noOfTeams);

        // First round has half the number of permutations as the number of teams.
        int noPermutations = _tournament.Groups.Count();

        Debug.WriteLine($"ex:{exponent}|byes:{noByes}|teams:{_teams.Count}|r:{noRounds}|perms:{noPermutations}");
        // Create first round matches
        int teamIndex = 0;
        for (int i = 0; i < noPermutations; i++)
        {
            var home = new Team();
            var away = new Team();

            home.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, teamIndex++);
            away.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, teamIndex++);

            // Only create matches for tennis tournaments (Sport == 1)
            if (_tournament.Sport == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, 1);
                permutation.Id = i;
                draw.AddPermutation(permutation, 1);
            }
        }
        //Passed group stages
        // From round 2 to the final round
        // Create placeholder matches with empty teams that will be populated as winners advance
        for (int r = 2; r <= noRounds; r++)
        {
            // In a knockout tournament, each round has half the number of matches as the previous round.
            // For example, 8 teams = 4 matches in the first round, 2 matches in the second round, and 1 match in the final.
            Debug.Write($"Round {r}:");
            noPermutations = (int)(noOfTeams / Math.Pow(2, r));

            // Create matches for this round  using placeholder teams
            for (int p = 0; p < noPermutations; p++)
            {
                var home = new Team();
                var away = new Team();
                home.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, teamIndex++);
                away.CreateBye(_tournament.Details.TeamSize, _tournament.Organization, teamIndex++);


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

    public override  void OnChange(Draw draw, int round, int previousRound, List<Score> scores)
    {
        //If rounds in the draw has playoff rounds, then pass
        //it to a OnChange handler that deals with playoff rounds
        if (draw.Group != null)
        {
            _drawMakerKOPlayoff.OnChange(draw, round, previousRound, scores);

        }
        //Transfer winners from groups into the main playoff draw

        //Check that all groups have completed their rounds

        bool groupsCompleted = _tournament.Groups.ToList().All(g =>
        {
            //Get the last round in the group draw
            var lastRound = g.Draw?.Rounds.OrderByDescending(r => r.Index).FirstOrDefault();
            var lastMatch = lastRound?.Permutations.FirstOrDefault()?.Matches.FirstOrDefault();
            //Group completed if both teams in the last match are not byes
            if (lastMatch == null || lastMatch.Home.FirstOrDefault()?.Bye == true || lastMatch.Away.FirstOrDefault()?.Bye == true)
                return false;
            return true; 
        });

        if (!groupsCompleted) return;
        //Need main draw
        if (draw.Group != null) return;

        //Check if the first round is scheduled.
        var round1 = draw.Rounds.FirstOrDefault(r => r.Index == 1);
        bool isScheduled = round1?.Permutations.All(p=> !p.Matches.First().Home.First().Bye && !p.Matches.First().Away.First().Bye) ?? false;

        if (!isScheduled)
        {
            //Make a list of home and away teams so that they don't play each other again
            List<Team> homeTeams = new();
            List<Team> awayTeams = new();
            foreach (var group in _tournament.Groups)
            {
                //Get the last round in the group draw
                var lastRound = group.Draw?.Rounds.OrderByDescending(r => r.Index).FirstOrDefault();
                var lastMatch = lastRound?.Permutations.FirstOrDefault()?.Matches.FirstOrDefault();
                if (lastMatch == null) continue;
                //Add home and away teams to the list
                if (lastMatch.Home.FirstOrDefault()?.Bye == false && lastMatch.Home.FirstOrDefault()?.Team != null)
                {
                    var team = lastMatch.Home.First().Team;
                    if (team != null)
                        homeTeams.Add(team);
                }
                if (lastMatch.Away.FirstOrDefault()?.Bye == false && lastMatch.Away.FirstOrDefault()?.Team != null)
                {
                    var team = lastMatch.Away.First().Team;
                    if (team != null)
                        awayTeams.Add(team);
                }
            }


            //Reverse away teams to match with home teams
            awayTeams.Reverse();

            //Get the first round
            var firstRound = draw.Rounds.FirstOrDefault(r => r.Index == 1);
            int teamIndex = 0;
            foreach (Permutation perm in firstRound?.Permutations ?? [])
            {
                var match = perm.Matches.First();
                if (teamIndex < homeTeams.Count) match.SetHomeSide(homeTeams[teamIndex]);
                if (teamIndex < awayTeams.Count) match.SetAwaySide(awayTeams[teamIndex]);

                teamIndex++;

            }

            if (scores.Count == 0) return;
        }
        


        //Progress the main draw
        _drawMakerKO.OnChange(draw, round, previousRound, scores);


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