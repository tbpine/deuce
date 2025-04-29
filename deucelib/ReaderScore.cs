namespace deuce;

/// <summary>
/// Read a match score in the format
/// of home_away : 6_4
/// </summary>
public class ReaderScore
{
    public ReaderScore(string score)
    {
    }


    private  List<int> ReadCsvNumbers(string csv)
    {
        return csv.Split(',')
                  .Select(int.Parse)
                  .ToList();
    }
}