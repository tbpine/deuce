namespace deuce;

/// <summary>
/// Combination and permutation methods.
/// </summary>
class PermsCombs
{
    /// <summary>
    /// Get all combinations of r numbers from the full
    /// set of n numbers.
    /// </summary>
    /// <param name="n">1..n integers</param>
    /// <returns>List of combinations</returns>
    public List<int[]> GetCombinations(int n)
    {
        //Result list.
        List<int[]> combs = new();

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                //Can't have combination with itself.
                if (i != j)
                {
                    var found = combs.Find(e => (e[0] == i && e[1] == j) || (e[1] == i && e[0] == j));
                    if (found is null) combs.Add(new int[] { i, j });
                }
            }
        }
    
        return combs;
    }
}