using System.Net;
namespace deuce.ext;


public static class ScheduleExt 
{
    public static int NoMatches (this Schedule s)
    {
        int total = 0;
        for(int i = 0; i < s.NoRounds; i++)
            total+=s.GetMatches(i)?.Count??0;
        return total;

    }
}