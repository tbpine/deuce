using System.Data.Common;

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
        return r.Next()%max;
    }

    private static T? GetRandomElement<T>(object[] objs) where T : class
    {

        Random r = new();
        return objs[r.Next() % objs.Length] as T;
    }

    private static string[] _team = new string[] { "Thunderbolts", "Wildcats", "Dragons", "Titans", "Mavericks", "Hurricanes", "Spartans", "Warriors", "Panthers", "Vipers", "Cyclones", "Knights", "Falcons", "Pioneers", "Gladiators", "Rangers", "Phoenix", "Cobras", "Lions", "Sharks", "Eagles", "Bears", "Wolves", "Tigers", "Bulls", "Stallions", "Raiders", "Pirates", "Samurai", "Ninjas", "Vikings", "Samurais", "Crusaders", "Rebels", "Outlaws", "Bandits", "Marauders", "Buccaneers", "Sentinels", "Guardians", "Defenders", "Avengers", "Champions", "Conquerors", "Invincibles", "Dynamos", "Juggernauts", "Behemoths", "Goliaths" };

    private static  string[] _players = new string[] {

            "John Smith", "Emily Johnson", "Michael Brown", "Sarah Davis", "David Wilson", "Jessica Martinez", "Daniel Anderson", "Laura Thomas", "James Taylor", "Olivia Harris", "Matthew Clark", "Sophia Lewis", "Christopher Walker", "Isabella Hall", "Joshua Allen", "Mia Young", "Andrew Hernandez", "Abigail King", "Joseph Wright", "Madison Lopez", "Ethan Hill", "Avery Scott", "Alexander Green", "Ella Adams", "Ryan Baker", "Grace Gonzalez", "Benjamin Nelson", "Chloe Carter", "Samuel Mitchell", "Lily Perez", "Gabriel Roberts", "Hannah Turner", "Anthony Phillips", "Zoe Campbell", "Jack Parker", "Victoria Evans", "Lucas Edwards", "Aria Collins", "Henry Stewart", "Scarlett Sanchez", "Isaac Morris", "Layla Rogers", "Nathan Reed", "Penelope Cook", "Christian Morgan", "Riley Bell", "Jonathan Murphy", "Nora Bailey", "Aaron Rivera", "Lillian Cooper", "Charles Richardson", "Emma Foster", "Mason Powell", "Aiden Hughes", "Sofia Price", "Jackson Russell", "Amelia Griffin", "Sebastian Hayes", "Harper Ward", "Elijah Torres", "Aubrey Peterson", "Logan Gray", "Evelyn Ramirez", "Caleb James", "Luna Brooks", "Dylan Kelly", "Mila Sanders", "Owen Bennett", "Stella Wood", "Wyatt Barnes", "Ellie Ross", "Carter Henderson", "Hazel Coleman", "Julian Jenkins", "Aurora Perry", "Levi Long", "Violet Patterson", "Lincoln Hughes", "Savannah Flores", "Hunter Simmons", "Brooklyn Butler", "Isaiah Foster", "Paisley Powell", "Grayson Howard", "Addison Ward", "Jaxon Torres", "Willow Peterson", "Josiah Gray", "Lucy Ramirez", "Nolan James", "Claire Brooks", "Easton Kelly", "Lydia Sanders", "Ezra Bennett", "Madeline Wood", "Carson Barnes", "Piper Ross", "Adrian Henderson", "Ruby Coleman", "Asher Jenkins", "Alice Perry"

        };


}