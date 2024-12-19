using System.Data;
using System.Diagnostics;

namespace deuce.lib;

/// <summary>
/// 
/// </summary>
class GameEngineRR : GameEngineBase, IGameEngine
{
    public GameEngineRR(Tournament t) : base(t)
    {
        this.GameCreated += GameEngine_GameCreated!;
    }

    public Dictionary<int, List<Game>> Generate(List<Player> players)
    {
        _players = players;
        //Add a bye for odd numbers
        if (_players.Count % 2 > 0)
            _players.Add(new Player { Id = -1, First = "BYE" });

        for (int i = 0; i < _players.Count; i++) _players[i].Index = i + 1;


        int noRounds = _players.Count - 1;
        int noGames = _players.Count / 2;

        for (int r = 0; r < noRounds; r++)
        {
            Debug.Write($"Round {r}:");

            for (int g = 0; g < noGames; g++)
            {
                Player home = _players[g];
                Player away = _players[players.Count - g - 1];

                Debug.Write("(" + home.Index + "," + away.Index + ")");
                RaiseGameCreatedEvent(r + 1, home.Index, away.Index);

            }

            Player pop = _players[0];
            _players.RemoveAt(0);
            _players.Add(pop);
            Debug.Write($"\n");
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