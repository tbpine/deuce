using System.Data.Common;
using deuce;

class RandomUtil
{

    public static string GetTeam()
    {
        return GetRandomElement<string>(_team) ?? "";
    }

    public static string GetPlayer()
    {
        return GetRandomElement<string>(_players) ?? "";
    }

    public static int GetInt(int max)
    {
        Random r = new();
        return r.Next() % max;
    }

    private static T? GetRandomElement<T>(object[] objs) where T : class
    {

        Random r = new();
        return objs[r.Next() % objs.Length] as T;
    }

    public static string GetNameAtIndex(int index)
    {
        return _players[index];
    }

    public static int GetNameCount() => _players.Length;

    // _team has 50 elements
    private static string[] _team = new string[] { "Thunderbolts", "Wildcats", "Dragons", "Titans", "Mavericks", "Hurricanes", "Spartans", "Warriors", "Panthers", "Vipers", "Cyclones", "Knights", "Falcons", "Pioneers", "Gladiators", "Rangers", "Phoenix", "Cobras", "Lions", "Sharks", "Eagles", "Bears", "Wolves", "Tigers", "Bulls", "Stallions", "Raiders", "Pirates", "Samurai", "Ninjas", "Vikings", "Samurais", "Crusaders", "Rebels", "Outlaws", "Bandits", "Marauders", "Buccaneers", "Sentinels", "Guardians", "Defenders", "Avengers", "Champions", "Conquerors", "Invincibles", "Dynamos", "Juggernauts", "Behemoths", "Goliaths" };

    // _players has 130 elements
    private static string[] _players = new string[] {

            "John Smith", "Emily Johnson", "Michael Brown", "Sarah Davis", "David Wilson", "Jessica Martinez", "Daniel Anderson", "Laura Thomas", "James Taylor", "Olivia Harris",
            "Matthew Clark", "Sophia Lewis", "Christopher Walker", "Isabella Hall", "Joshua Allen", "Mia Young", "Andrew Hernandez", "Abigail King", "Joseph Wright", "Madison Lopez",
            "Ethan Hill", "Avery Scott", "Alexander Green", "Ella Adams", "Ryan Baker", "Grace Gonzalez", "Benjamin Nelson", "Chloe Carter", "Samuel Mitchell", "Lily Perez",
            "Gabriel Roberts", "Hannah Turner", "Anthony Phillips", "Zoe Campbell", "Jack Parker", "Victoria Evans", "Lucas Edwards", "Aria Collins", "Henry Stewart", "Scarlett Sanchez",
            "Isaac Morris", "Layla Rogers", "Nathan Reed", "Penelope Cook", "Christian Morgan", "Riley Bell", "Jonathan Murphy", "Nora Bailey", "Aaron Rivera", "Lillian Cooper",
            "Charles Richardson", "Emma Foster", "Mason Powell", "Aiden Hughes", "Sofia Price", "Jackson Russell", "Amelia Griffin", "Sebastian Hayes", "Harper Ward", "Elijah Torres",
            "Aubrey Peterson", "Logan Gray", "Evelyn Ramirez", "Caleb James", "Luna Brooks", "Dylan Kelly", "Mila Sanders", "Owen Bennett", "Stella Wood", "Wyatt Barnes",
            "Ellie Ross", "Carter Henderson", "Hazel Coleman", "Julian Jenkins", "Aurora Perry", "Levi Long", "Violet Patterson", "Lincoln Hughes", "Savannah Flores", "Hunter Simmons",
            "Brooklyn Butler", "Isaiah Foster", "Paisley Powell", "Grayson Howard", "Addison Ward", "Jaxon Torres", "Willow Peterson", "Josiah Gray", "Lucy Ramirez", "Nolan James",
            "Claire Brooks", "Easton Kelly", "Lydia Sanders", "Ezra Bennett", "Madeline Wood", "Carson Barnes", "Piper Ross", "Adrian Henderson", "Ruby Coleman", "Asher Jenkins",
            "Alice Perry", "Marcus Thompson", "Natalie White", "Felix Rodriguez", "Cora Martinez", "Blake Sullivan", "Iris Chen", "Cameron Foster", "Delilah Murphy", "Tyler Walsh",
            "Sienna Parker", "Dominic Rivera", "Jade Harrison", "Knox Williams", "Autumn Davis", "Phoenix Lee", "Melody Chang", "Finn O'Connor", "Dahlia Patel", "River Stone",
            "Celeste Garcia", "Atlas Kim", "Sage Mitchell", "Orion Taylor", "Ivy Jackson", "Jasper Wilson", "Nova Thompson", "Reed Martinez", "Willow Chen", "Cruz Rodriguez"

        };

    //make a randome tornament name
    public static string GetRandomTournamentName()
    {
        Random r = new();
        return "Tournament_" + r.Next(1000, 9999);
    }

    /// <summary>
    /// Get a list of random unique players
    /// </summary>
    /// <param name="count">Number of players to get</param>
    /// <returns>List of random unique players</returns>
    public static List<Player> GetRandomPlayers(int count)
    {
        // Create unique random players by shuffling indices
        var players = new List<Player>();
        var availableIndices = Enumerable.Range(0, GetNameCount()).ToList();

        // Shuffle the indices to ensure random selection without duplicates
        Random random = new Random();
        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (availableIndices[i], availableIndices[j]) = (availableIndices[j], availableIndices[i]);
        }

        // Take the first noPlayers indices and create players
        for (int i = 0; i < count; i++)
        {
            var nameParts = GetNameAtIndex(availableIndices[i]).Split(' ');
            players.Add(new Player
            {
                Id = i + 1,
                First = nameParts[0],
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown",
                Ranking = random.NextDouble() * 100 // Add random ranking for testing
            });
        }

        return players;    
    }

}