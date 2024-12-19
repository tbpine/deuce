using System.Data;

//Find the number of combinations
//for the number of players.
int n = 10;
List<int[]> combs = new();

for (int i = 1; i <= n; i++)
{
    for (int j = 1; j <= n; j++)
    {
        if (i != j)
        {
            var found = combs.Find(e => (e[0] == i && e[1] == j) || (e[1] == i && e[0] == j));
            if (found is null) combs.Add(new int[] { i, j });
        }
    }
}

foreach (int[] comb in combs) { Console.WriteLine(comb[0].ToString() + "," + comb[1].ToString()); }
Console.WriteLine($"No combinations = {combs.Count}.");

//Need to select n/2 combinations involving
//all numbers

List<int> participents = new();

int idx = 0;

Random rand = new Random((int)DateTime.Now.Ticks);

for (int r = 0; r < (n - 1) && combs.Count > 0; r++)
{
    Console.Write($"Round {r + 1}:");

    // for (int g = 0; g < (n / 2); g++)
    // {
    List<int[]> delIndx = new();
    //Player selection
    bool full = false;
    List<int> tried = new();

    while (!full)
    {
        idx = combs.Count > 1 ? rand.Next() % combs.Count() : 0;
        while (tried.Contains(idx))
            idx = combs.Count > 1 ? rand.Next() % combs.Count() : 0;

        if (!participents.Contains(combs[idx][0]) && !participents.Contains(combs[idx][1]))
        {
            int lhs = combs[idx][0];
            int rhs = combs[idx][1];
            Console.Write("(" + lhs + "," + rhs + ")");
            participents.Add(lhs);
            participents.Add(rhs);

            delIndx.Add(new int[] { lhs, rhs });
            //combs.RemoveAll(e => e[0] == lhs && e[1] == rhs);

        }
        else
            tried.Add(idx);

        full = participents.Count >= n;

        if (tried.Count == combs.Count)
        {
            //Tried all combo, rest
            participents.Clear();
            tried.Clear();
            delIndx.Clear();
            Console.Write("|");
        }
    }
    
    foreach (int[] d in delIndx) combs.RemoveAll(e => e[0] == d[0] && e[1] == d[1]);
    
    // for (; idx < combs.Count; idx++)
    // {
    //     if (!participents.Contains(combs[idx][0]) && !participents.Contains(combs[idx][1]))
    //     {
    //         Console.Write("(" + combs[idx][0] + "," + combs[idx][1] + ")");
    //         participents.Add(combs[idx][0]);
    //         participents.Add(combs[idx][1]);
    //         delIndx.Add(new int[] { combs[idx][0], combs[idx][1]});
    //     }

    //     if (participents.Count == n) break;
    // }

    // foreach (int[] d in delIndx) combs.RemoveAll(e => e[0] == d[0] && e[1] == d[1]);

    // }

    participents.Clear();
    idx = 0;
    Console.Write("\n");
}

