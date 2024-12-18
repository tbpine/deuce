int n = 8;

List<Player> players = new();
for (int i = 0; i < n; i++)
    players.Add(new Player { Name = $"player_{i}" });

int noRounds = n - 1;
Random r = new Random((int)DateTime.Now.Ticks);

for (int i = 0; i < noRounds; i++)
{

    List<Player> pool = new(players);
    Console.Write($"Round {i+1}:");
    while (pool.Count > 0)
    {
        Player lhs = pool[r.Next() % pool.Count];
        Player? rhs = null;
        foreach (Player iter in pool)
        {
            if (!iter.Equals(lhs) && !iter.Opponents.Contains(lhs))
            {
                lhs.Opponents.Add(iter);
                iter.Opponents.Add(lhs);
                rhs = iter;
                Console.Write($"({lhs.Name},{rhs.Name}),");
                break;
            }

        }

        pool.Remove(lhs);
        if (rhs is not null) pool.Remove(rhs);

    }

    Console.WriteLine();
}
