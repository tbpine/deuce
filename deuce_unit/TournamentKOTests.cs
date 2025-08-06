using deuce;
using System.Linq;
namespace deuce_unit;

[TestClass]
public class TournamentKOTests
{
    public TestContext? TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        // Code to initialize before each test
    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(16)]
    [DataRow(32)]
    [DataRow(64)]
    [DataRow(128)]

    public void print_ko_tournament(int noPlayers)
    {
        // Create unique random players by shuffling indices
        var players = new List<Player>();
        var availableIndices = Enumerable.Range(0, RandomUtil.GetNameCount()).ToList();
        
        // Shuffle the indices to ensure random selection without duplicates
        Random random = new Random();
        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (availableIndices[i], availableIndices[j]) = (availableIndices[j], availableIndices[i]);
        }
        
        // Take the first noPlayers indices and create players
        for (int i = 0; i < noPlayers; i++)
        {
            var nameParts = RandomUtil.GetNameAtIndex(availableIndices[i]).Split(' ');
            players.Add(new Player
            {
                Id = i + 1,
                First = nameParts[0],
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown"
            });
        }

        // Create 8 teams and assign one player to each team
        var teams = new List<Team>();
        for (int i = 0; i < noPlayers; i++)
        {
            var team = new Team
            {
                Id = i + 1,
                Label = RandomUtil.GetTeam()
            };
            team.AddPlayer(players[i]);
            teams.Add(team);
        }

        // Initialize organization and database connection
        Organization organization = new Organization() { Id = 100, Name = "Test Organization" };

        // Create a KO tournament
        AssignTournament assignTournament = new();
        Tournament tournament = assignTournament.MakeRandom(
            tournamentType: 2, // KO tournament type
            label: RandomUtil.GetRandomTournamentName(),
            noPlayers: players.Count,
            sport: 1, // Example sport ID
            noSingle: 1,
            noDouble: 0,
            sets: 1,
            teamSize: 1,
            players: players


        );

        //Draw should not be null
        Assert.IsNotNull(tournament.Draw, "Draw should not be null");

        //Assig ids to matches, permutations and rounds so scoring can
        //be linked to them
        int matchId = 1, permId = 1;
        //Store a list of scores
        List<Score> scores = new List<Score>();
        foreach (Round round in tournament.Draw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                permutation.Id = permId++;
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;

                    //A a random score for each set in the match
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        Score score = new Score
                        {
                            Id = match.Id,
                            Home = RandomUtil.GetInt(6),
                            Away = RandomUtil.GetInt(6),
                            Permutation = permutation.Id,
                            Match = match.Id,
                            Round = round.Index,
                            Set = i + 1,
                        };

                        scores.Add(score);

                    }
                }
            }
        }
        // Print schedule to PDF

        string filename = $"{tournament.Label}_Round.pdf";
        using FileStream pdfFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new PdfPrinter(tournament.Draw, new PDFTemplateFactory());
        printer.Print(pdfFile, tournament, tournament.Draw, 1, scores);


    }

    [TestMethod]
    [DataRow(8)]
    [DataRow(16)]
    [DataRow(32)]
    [DataRow(64)]
    [DataRow(128)]
    public void print_ko_tournament_with_advancement(int noPlayers)
    {
        // Create unique random players by shuffling indices
        var players = new List<Player>();
        var availableIndices = Enumerable.Range(0, RandomUtil.GetNameCount()).ToList();
        
        // Shuffle the indices to ensure random selection without duplicates
        Random random = new Random();
        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (availableIndices[i], availableIndices[j]) = (availableIndices[j], availableIndices[i]);
        }
        
        // Take the first noPlayers indices and create players
        for (int i = 0; i < noPlayers; i++)
        {
            var nameParts = RandomUtil.GetNameAtIndex(availableIndices[i]).Split(' ');
            players.Add(new Player
            {
                Id = i + 1,
                First = nameParts[0],
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown"
            });
        }

        // Create 8 teams and assign one player to each team
        var teams = new List<Team>();
        for (int i = 0; i < noPlayers; i++)
        {
            var team = new Team
            {
                Id = i + 1,
                Label = ""
            };
            team.AddPlayer(players[i]);
            teams.Add(team);
        }

        // Initialize organization and database connection
        Organization organization = new Organization() { Id = 100, Name = "Test Organization" };

        // Create a KO tournament
        AssignTournament assignTournament = new();
        Tournament tournament = assignTournament.MakeRandom(
            tournamentType: 2, // KO tournament type
            label: RandomUtil.GetRandomTournamentName(),
            noPlayers: players.Count,
            sport: 1, // Example sport ID
            noSingle: 1,
            noDouble: 0,
            sets: 1,
            teamSize: 1,
            players: players


        );

        //Draw should not be null
        Assert.IsNotNull(tournament.Draw, "Draw should not be null");

        //Assig ids to matches, permutations and rounds so scoring can
        //be linked to them
        int matchId = 1, permId = 1;
        //Store a list of scores
        List<Score> scores = new List<Score>();
        foreach (Round round in tournament.Draw.Rounds)
        {
            foreach (Permutation permutation in round.Permutations)
            {
                permutation.Id = permId++;
                foreach (Match match in permutation.Matches)
                {
                    match.Id = matchId++;

                    //A a random score for each set in the match
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        //Randomly who wins the match
                        bool homeWins = RandomUtil.GetInt(2) == 0;

                        Score score = new Score
                        {
                            Id = match.Id,
                            Home = homeWins ? 6 : RandomUtil.GetInt(4),
                            Away = !homeWins ? 6 : RandomUtil.GetInt(4),
                            Permutation = permutation.Id,
                            Match = match.Id,
                            Round = round.Index,
                            Set = i + 1,
                        };

                        scores.Add(score);

                    }
                }
            }
        }

        //Draw advancement for the tournament
        FactoryDrawMaker fac = new FactoryDrawMaker();
        IGameMaker gm = new GameMakerTennis();
        IDrawMaker scheduler = fac.Create(tournament, gm);
        for (int t = 2; t <= tournament.Draw.Rounds.Count; t++)
        {
            //Advance tournament
            scheduler.OnChange(tournament.Draw, t, t - 1, scores);
        }

        // Print schedule to PDF
        string filename = $"{tournament.Label}_Round.pdf";
        using FileStream pdfFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new PdfPrinter(tournament.Draw, new PDFTemplateFactory());
        printer.Print(pdfFile, tournament, tournament.Draw, 1, scores);


    }


    [TestMethod]
    public void TestTemplate()
    {
        // Create unique random players by shuffling indices
        var players = new List<Player>();
        var availableIndices = Enumerable.Range(0, RandomUtil.GetNameCount()).ToList();
        
        // Shuffle the indices to ensure random selection without duplicates
        Random random = new Random();
        for (int i = availableIndices.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (availableIndices[i], availableIndices[j]) = (availableIndices[j], availableIndices[i]);
        }
        
        // Take the first 8 indices and create players
        for (int i = 0; i < 8; i++)
        {
            var nameParts = RandomUtil.GetNameAtIndex(availableIndices[i]).Split(' ');
            players.Add(new Player
            {
                Id = i,
                First = nameParts[0],
                Last = nameParts.Length > 1 ? nameParts[1] : "Unknown"
            });
        }

        // Create 8 teams and assign one player to each team
        var teams = new List<Team>();
        for (int i = 0; i < 8; i++)
        {
            var team = new Team
            {
                Id = i,
                Label = RandomUtil.GetTeam()
            };
            team.AddPlayer(players[i]);
            teams.Add(team);
        }

        // Initialize organization and database connection
        Organization organization = new Organization() { Id = 100, Name = "Test Organization" };

        // Create a KO tournament
        AssignTournament assignTournament = new();
        Tournament tournament = assignTournament.MakeRandom(
            tournamentType: 3, //test
            label: RandomUtil.GetRandomTournamentName(),
            noPlayers: players.Count,
            sport: 1, // Example sport ID
            noSingle: 1,
            noDouble: 0,
            sets: 1,
            teamSize: 1,
            players: players

        );

        // Print draw to PDF
        Assert.IsNotNull(tournament.Draw, "Draw should not be null");

        string filename = $"{tournament.Label}_Round_.pdf";
        using FileStream pdfFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new PdfPrinter(tournament.Draw, new PDFTemplateFactory());
        printer.Print(pdfFile, tournament, tournament.Draw, 1);
    }

}
