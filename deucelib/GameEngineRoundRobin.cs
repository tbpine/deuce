using System.Data.SqlTypes;
using deuce.lib;

class GameEngineRoundRobin : GameEngineBase, IGameEngine
{
    public GameEngineRoundRobin(Tournament t) : base(t) { }

    public Dictionary<int,List<Game>> Generate(List<Player> players)
    {
        if (players.Count % 2 != 0)
            players.Add(new Player()
            {
                First = "BYE",
                Id = -1
            });

        Random r = new Random((int)DateTime.Now.Ticks);

        for (int i = 0;  i < players.Count - 1; i++)
        {
            //For each player, check which person he
            //hasn't played this round
            List<Player> rpool = new(players);

            for (int j = 0; j < (players.Count /2); j++)
            {
                Player lhs = rpool[0];
                //Find a match for player 
                var selList= Player.ExcList(lhs, rpool);
                int rIdx = rpool.Count > 2 ? r.Next() % selList.Count : 1;
                Player rhs = selList[rIdx];

                //make game

                Game g = new("", j, lhs,  rhs);
                lhs.AddGame(g);
                rhs.AddGame(g);
                //Shrink pool

                rpool.RemoveAll(e => e.Id == lhs.Id || e.Id == rhs.Id);
            }
            

           
        }

        //Game aggregation
        Dictionary<int, List<Game>> results = new();

        for (int round = 0; round < players.Count - 1; round++)
        {
            List<Game> roundGames = new();
            foreach (Player p in players)
            {
                var q = from g in p.Games where g.Round == round select g;
                //Select distinct games.
                if (roundGames.Find(e=>e.IsSameGame(q.First())) == null)
                    roundGames.Add(q.First());
            }

            results.Add(round, roundGames);
        }

        return results;
    }
}