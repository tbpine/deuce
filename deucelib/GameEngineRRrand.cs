using System.Diagnostics;
using System.Text;

namespace deuce.lib;

/// <summary>
/// Method:
/// Get all possible combinations
/// For each round,
/// Keep selecting combinations until  all participant are involved
/// Remove combinations that were selected
/// Repeat for each round.
/// </summary>
public class GameEngineRRrand : GameEngineBase, IGameEngine
{
    //Keep a reference to the list of players.
    private List<Player>? _players;

    private Dictionary<int, List<Game>> _results = new();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    public GameEngineRRrand(Tournament t) : base(t)
    {
        this.GameCreated += GameEngine_GameCreated!;
    }



    public Dictionary<int, List<Game>> Generate(List<Player> players)
    {
        _players = players;
        //Index players
        for (int i = 0; i < _players.Count; i++) _players[i].Index = i + 1;

        int n = players.Count;
        PermsCombs permsCombs = new PermsCombs();

        //Find the number of combinations
        //for the number of players.
        List<int[]> combs = permsCombs.GetCombinations(n);

        //Need to select n/2 combinations involving
        //all numbers

        List<int> participents = new();

        int idx = 0;

        Random rand = new Random((int)DateTime.Now.Ticks);

        //Number of rounds equals number of players -1
        //because everyone has to play each other.
        for (int r = 0; r < (n - 1) && combs.Count > 0; r++)
        {

#if DEBUG
            StringBuilder line = new();
#endif

#if DEBUG
            line.Append($"Round {r + 1}:");
#else
            Debug.Write($"Round {r + 1}:");
#endif

            List<int[]> delIndx = new();
            //Player selection
            bool roundSheduled = false;
            List<int> history = new();
            while (!roundSheduled)
            {
                //Randomize player selection
                //Unless there's only 1 combination left.
                idx = combs.Count > 1 ? rand.Next() % combs.Count() : 0;

                //Try another index if been tried already
                //or it's in the list combinations to be deleted.
                bool keepTryingIndexes = history.Contains(idx) &&
                delIndx.Find(e => e[0] == combs[idx][0] && e[1] == combs[idx][1]) == null;

                //Keep trying new combinations.
                while (history.Contains(idx))
                    idx = combs.Count > 1 ? rand.Next() % combs.Count() : 0;

                if (!participents.Contains(combs[idx][0]) && !participents.Contains(combs[idx][1]))
                {
                    int lhs = combs[idx][0];
                    int rhs = combs[idx][1];
#if DEBUG
                    line.Append("(" + lhs + "," + rhs + ")");
#else
                    Debug.Write("(" + lhs + "," + rhs + ")");
#endif
                    //Make game
                    RaiseGameCreatedEvent(r, lhs, rhs);

                    participents.Add(lhs);
                    participents.Add(rhs);

                    delIndx.Add(new int[] { lhs, rhs });

                }
                else
                {
                    //This combination was tried,
                    //but participants are in other games.
                    history.Add(idx);
                }

                roundSheduled = participents.Count >= n;

                //All combinations were tried.
                //Try again
                if (history.Count == combs.Count)
                {
                    //Tried all combo, rest
                    participents.Clear();
                    history.Clear();
                    delIndx.Clear();
#if DEBUG
                    line.Clear();
                    line.Append($"Round {r + 1}:");
#endif
                    //Debug.Write("|");
                }
            }

            foreach (int[] d in delIndx) combs.RemoveAll(e => e[0] == d[0] && e[1] == d[1]);
            participents.Clear();
            idx = 0;
#if DEBUG
            Debug.WriteLine(line);
#else
            Debug.Write("\n");
#endif
        }

        return _results;
    }

    private void GameEngine_GameCreated(object sender, GameCreatedEventArgs args)
    {
        List<Game> round = _results.ContainsKey(args.Round) ? _results[args.Round] :
        new List<Game>();

        if (!_results.ContainsKey(args.Round)) _results.Add(args.Round, round);

        //Find players using indexes.
        Player? lhsPlayer = _players?.Find(e => e.Index == args.Lhs);
        Player? rhsPlayer = _players?.Find(e => e.Index == args.Rhs);

        //Make the games
        Game game = new Game("", args.Round, new Player[] { lhsPlayer!, rhsPlayer! });
        round.Add(game);


    }
}