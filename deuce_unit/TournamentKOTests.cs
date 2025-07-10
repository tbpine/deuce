using Microsoft.VisualStudio.TestTools.UnitTesting;
using deuce;
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
    public void CreatePlayersAndTeams()
    {
        // Create 8 random players
        var players = new List<Player>();
        for (int i = 0; i < 8; i++)
        {
            var nameParts = RandomUtil.GetPlayer().Split(' ');
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

        //Schedule should not be null
        Assert.IsNotNull(tournament.Schedule, "Schedule should not be null");
        // Print schedule to PDF
        
        string filename = $"{tournament.Label}_Round.pdf";
        using FileStream pdfFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new PdfPrinter(tournament.Schedule, new TemplateFactory());
        printer.Print(pdfFile, tournament, tournament.Schedule, 1);


    }


    [TestMethod]
    public void TestTemplate()
    {
        // Create 8 random players
        var players = new List<Player>();
        for (int i = 0; i < 8; i++)
        {
            var nameParts = RandomUtil.GetPlayer().Split(' ');
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

        // Print schedule to PDF
        Assert.IsNotNull(tournament.Schedule, "Schedule should not be null");
        
        string filename = $"{tournament.Label}_Round_.pdf";
        using FileStream pdfFile = new FileStream(filename, FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new PdfPrinter(tournament.Schedule, new TemplateFactory());
        printer.Print(pdfFile, tournament, tournament.Schedule, 1);
    }

}
