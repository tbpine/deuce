class Program1
{
    public void Main()
    {
        int n = 8;

        List<Player> players = new();
        for (int i = 0; i < n; i++)
            players.Add(new Player { Name = $"player_{i + 1}" });

        int noRounds = n - 1;
        Random r = new Random((int)DateTime.Now.Ticks);
        int noGames = n / 2;
        bool randomSelect = true;
        int tries = 0;

        for (int i = 0; i < noRounds; i++)
        {

            List<Player> pool = new(players);
            Console.Write($"Round {i + 1}:");
            //Need to make sure there's noGames
            //produce
            List<Player[]> roundBuffer = new();

            while (roundBuffer.Count < noGames)
            {
                int selIdx = randomSelect ? r.Next() % pool.Count : 0;
                Player lhs = pool[selIdx];
                Player? rhs = null;
                foreach (Player iter in pool)
                {
                    //If it's not the selected player, and
                    //they havn't played, then create a game.
                    if (!iter.Equals(lhs) && !iter.Opponents.Contains(lhs))
                    {
                        rhs = iter;
                        roundBuffer.Add(new Player[] { lhs, rhs! });
                        // lhs.Opponents.Add(rhs);
                        // rhs.Opponents.Add(lhs);

                        pool.RemoveAll(e => e.Equals(lhs) || e.Equals(rhs));

                        break;
                    }

                }
                //Could find an opponent so reset
                if (rhs is null)
                {
                    //Reset
                    pool = new(players);
                    roundBuffer.Clear();
                    r = new Random((int)DateTime.Now.Ticks);
                    tries++;
                }


            }


            //Commit Round
            foreach (var game in roundBuffer)
            {
                game[0].Opponents.Add(game[1]);
                game[1].Opponents.Add(game[0]);
                Console.Write($"({game[0]},{game[1]}),");
            }

            Console.WriteLine();

        }
    }
}