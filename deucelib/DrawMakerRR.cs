using System.Diagnostics;

namespace deuce;

/// <summary>
/// Round robin format
/// </summary>
class DrawMakerRR : DrawMakerBase, IDrawMaker
{
    private readonly IGameMaker _gameMaker;
    public DrawMakerRR(Tournament t, IGameMaker gameMaker) : base(t)
    {
        _gameMaker = gameMaker;
    }

    public Draw Run(List<Team> teams)
    {
        //The result
        Draw draw = new Draw(_tournament);

        //Assigns
        _teams = teams;
        //Add a bye for odd numbers
        if (_teams.Count % 2 > 0)
            _teams.Add(new Team(-1, "BYE"));

        for (int i = 0; i < _teams.Count; i++) _teams[i].Index = i + 1;

        //It work out this way
        int noRounds = _teams.Count - 1;
        int noPermutations = _teams.Count / 2;

        for (int r = 0; r < noRounds; r++)
        {
            Debug.Write($"Round {r}:");

            for (int p = 0; p < noPermutations; p++)
            {
                Team home = _teams[p];
                Team away = _teams[teams.Count - p - 1];

                Debug.Write("(" + home.Index + "," + away.Index + ")");

                //Schedule matches between each team.
                if (_tournament.Sport == 1)
                {
                    var permutation = _gameMaker.Create(_tournament, home, away, r);
                    permutation.Id = p;
                    draw.AddPermutation(permutation, r);
                }

            }
            //Next Round
            var pop = _teams[0];
            _teams.RemoveAt(0);
            _teams.Add(pop);
            Debug.Write($"\n");
        }



        return draw;
    }

    /// <summary>
    /// Executed before the end of a round.
    /// This is where you can do any pre-round end processing, such as updating scores or checking
    /// </summary>
    /// <param name="schedule"> The current schedule</param>
    /// <param name="round"> The current round number</param>
    public void BeforeEndRound(Draw schedule, int round, List<Score> scores)
    {
        //Nothing to do here
    }

    /// <summary>
    /// Progress to the next round of the schedule.
    /// </summary>
    /// <param name="schedule" > The current schedule</param>
    /// <param name="round"> The current round number</param>
    public void NextRound(Draw schedule, int round, int previousRound, List<Score> scores)
    {
        //Nothing to do here
    }

}