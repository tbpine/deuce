using System.Diagnostics;
using deuce.lib;

class GameEngineRRLinear : GameEngineBase, IGameEngine
{

    public GameEngineRRLinear(Tournament t) : base(t)
    {
        this.GameCreated += GameEngine_GameCreated!;
    }

    public Dictionary<int, List<Game>> Generate(List<Player> players)
    {
        _players = players;
        //Index players
        for (int i = 0; i < _players.Count; i++) _players[i].Index = i + 1;

        //Find the number of combinations
        //for the number of players.
        int n = players.Count;
        PermsCombs permsCombs = new PermsCombs();
        List<int[]> combs = permsCombs.GetCombinations(n);

        foreach (int[] comb in combs) { Debug.WriteLine(comb[0].ToString() + "," + comb[1].ToString()); }
        Debug.WriteLine($"No combinations = {combs.Count}.");

        //Need to select n/2 combinations involving
        //all numbers

        List<int> participents = new();

        int idx = 0;
        //Rounds
        for (int r = 0; r < (n - 1) && combs.Count > 0; r++)
        {
            Debug.Write($"Round {r + 1}:");
            List<int[]> delIndx = new();

            for (int g = 0; g < (n / 2); g++)
            {
                //Player selection

                for (; idx < combs.Count; idx++)
                {
                    if (!participents.Contains(combs[idx][0]) && !participents.Contains(combs[idx][1]))
                    {
                        int lhs = combs[idx][0];
                        int rhs = combs[idx][1];
                        Debug.Write("(" + lhs + "," + rhs + ")");
                        participents.Add(lhs);
                        participents.Add(rhs);
                        RaiseGameCreatedEvent(r, lhs, rhs);
                        delIndx.Add(new int[] { lhs, rhs });
                        break;
                    }

                    //   if (participents.Count == n) break;
                }
            }

            foreach (int[] d in delIndx) combs.RemoveAll(e => e[0] == d[0] && e[1] == d[1]);

            participents.Clear();
            idx = 0;
            Debug.Write("\n");

        }//Rounds.


        return _results;
    }

    private void GameEngine_GameCreated(object sender, GameCreatedEventArgs args)
    {
        if (!_results.ContainsKey(args.Round)) _results.Add(args.Round, new List<Game>());

        List<Game> round = _results[args.Round];
        //Find players using indexes.
        Player? lhsPlayer = _players?.Find(e => e.Index == args.Lhs);
        Player? rhsPlayer = _players?.Find(e => e.Index == args.Rhs);

        //Make the games
        Game game = new Game("", args.Round, new Player[] { lhsPlayer!, rhsPlayer! });
        round.Add(game);


    }
}