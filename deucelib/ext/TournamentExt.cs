namespace deuce.ext;

public static class TournamentExt
{
    /// <summary>
    /// Check if the tournament is new.
    /// </summary>
    /// <param name="tournament">Tournamen object</param>
    /// <returns></returns>
    public static bool IsNew(this Tournament tournament)
    {
        return tournament.Id == 0;
    }

    public static List<Team> CreateByeTeams(this Tournament tournament, int size)
    {
        //Create a list of teams to return
        List<Team> byeTeams = new();
        //Loop through 0 to size-1
        for (int i = 0; i < size; i++) byeTeams.Add(new Team(i, "BYE", tournament.Details.TeamSize));

        return byeTeams;
    }

    /// <summary>
    ///  Create a new TournamentDetail based on the tournament type.
    /// Assumption , sport is tennis
    /// </summary>
    /// <param name="tournamentType"></param>
    /// <returns></returns>
    public static void CreateDetail(this Tournament t)
    {
        //Default to tennis for now
        t.Details = CreateDetailsForTennis(t.Type);

    }


    /// <summary>
    ///  Create a new TournamentDetail based on the tournament type.
    /// </summary>
    /// <param name="tournamentType">Tournament type to create details for.</param>
    /// <returns></returns>
    private static TournamentDetail CreateDetailsForTennis(int tourTypeId)
    {
        //For round robin, set specific values
        if (tourTypeId  == 1) // Assuming 1 is the ID for round robin
        {
            return new TournamentDetail
            {
                Sets = 1,
                Games = 6,
                NoSingles = 2,
                NoDoubles = 2,
                TeamSize = 2 // Default team size for round robin
            };
        }

        //All other tournament types
        return new TournamentDetail
        {
            Sets = 1,
            Games = 6,
            NoSingles = 1,
            NoDoubles = 0,
            TeamSize = 1
        };
    }
}