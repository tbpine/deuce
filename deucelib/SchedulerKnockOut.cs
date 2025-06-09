using System.Diagnostics;

namespace deuce;

/// <summary>
/// Round robin format
/// </summary>
class SchedulerKnockOut : SchedulerBase, IScheduler
{
    private readonly IGameMaker _gameMaker;
    public SchedulerKnockOut(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    public Schedule Run(List<Team> teams)
    {
        //The result
        Schedule schedule = new Schedule(_tournament);

        //Assigns
        _teams = teams;
        //Add a byes for number of teams that is not a power of 2.
        //Work out the number of byes needed.
        int exponent = (int)Math.Ceiling(Math.Log2(_teams.Count));
        int noByes = (int)Math.Pow(2, exponent) - teams.Count;

        // Add noByes to the teams list
        for (int i = 0; i < noByes; i++) _teams.Add(new Team(0, "BYE", _tournament.Details.TeamSize));

        //Sort teams by ranking decending
        _teams.Sort((x, y) => (int)(y.Ranking - x.Ranking));
        //Assign indices to teams
        for (int i = 0; i < _teams.Count; i++) _teams[i].Index = i + 1;

        //work out rounds in a knockout tournament
        //The number of rounds is log2(teams)
        //For example, 8 teams = 3 rounds, 16 teams = 4 rounds, etc.

        int noRounds = (int)Math.Log2(_teams.Count);
        //First round has half the number of permutations as the number of teams.
        int noPermutations = _teams.Count /2 ;
        //for each permutation, an element in the top half of "_teams" 
        //plays against an element in the bottom half of "_teams".
        Debug.WriteLine($"ex:{exponent}|byes:{noByes}|teams:{_teams.Count}|r:{noRounds}|perms:{noPermutations}");
        for(int i = 0; i < noPermutations; i++)
        {
            var home = _teams[i];
            var away = _teams[_teams.Count - i - 1];

            if  (_tournament.Sport  == 1)
            {
                var permutation = _gameMaker.Create(_tournament, home, away, 1);
                permutation.Id = i;
                schedule.AddPermutation(permutation, 1);
            }


        }

        //From round 2 to the final round
        for(int r = 2; r <= noRounds; r++)
        {
            //In a knockout tournament, each round has half the number of matches as the previous round.
            //For example, 8 teams = 4 matches in the first round, 2 matches in the second round, and 1 match in the final.
            Debug.Write($"Round {r}:");
             noPermutations = (int)( _teams.Count / Math.Pow(2, r));
            //Make an list empty teams
            List<Team> emptyTeams = new List<Team>();
            for(int i =0; i < noPermutations; i++)
            {
                var home = new Team(0, "") { Index = i };
                var away = new Team(0, "") { Index = i + 1 };
                for(int j = 0; j < _tournament.Details.TeamSize; j++)
                {
                    home.AddPlayer(new Player() {  Index = j });
                    away.AddPlayer(new Player() { Index = j });
                }
                emptyTeams.Add(home);
                emptyTeams.Add(away);
            }

            for (int p = 0; p < noPermutations; p++)
            {

                var home = emptyTeams[p];
                var away = emptyTeams[emptyTeams.Count - p - 1];
                Debug.Write("(" + home.Index + "," + away.Index + ")");
                

                //Schedule matches between each team.
                if (_tournament.Sport == 1)
                {
                    var permutation = _gameMaker.Create(_tournament, home, away, r);
                    permutation.Id = p;
                    schedule.AddPermutation(permutation, r);
                }
           
            }
          
            Debug.Write($"\n");
        }

     


        return schedule;
    }


}