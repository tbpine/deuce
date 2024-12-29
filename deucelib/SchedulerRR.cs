using System.Data;
using System.Diagnostics;

namespace deuce;

/// <summary>
/// Round robin format
/// </summary>
class SchedulerRR : SchedulerBase, IScheduler
{
    private readonly IGameMaker _gameMaker;
    public SchedulerRR(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    public Schedule Run(List<Team> teams)
    {
        //The result
        Schedule schedule = new Schedule(_tournament);

        //Assigns
        _teams = teams;
        //Add a bye for odd numbers
        if (_teams.Count % 2 > 0)
            _teams.Add(new Team (-1, "BYE"));

        for (int i = 0; i < _teams.Count; i++) _teams[i].Index = i + 1;


        int noRounds = _teams.Count - 1;
        int noGames = _teams.Count / 2;

        for (int r = 0; r < noRounds; r++)
        {
            Debug.Write($"Round {r}:");

            for (int g = 0; g < noGames; g++)
            {
                Team home = _teams[g];
                Team away = _teams[teams.Count - g - 1];

                Debug.Write("(" + home.Index + "," + away.Index + ")");

                //Schedule matches between each team.
                if  (_tournament.Sport == 1)
                {
                    var permutation = _gameMaker.Create(_tournament, home, away, r);
                    schedule.AddPermutation(permutation, r);
                }
           
            }
            //Next Round
            var pop = _teams[0];
            _teams.RemoveAt(0);
            _teams.Add(pop);
            Debug.Write($"\n");
        }



        return schedule;
    }


}