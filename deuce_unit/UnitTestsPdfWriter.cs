using deuce;

namespace deuce_unit;

[TestClass]
public class UnitsPdfWriter
{

    [TestMethod]
    [DataRow(8)]
    public void Print_Robbin_Single_Player(int noPlayers)
    {
        //Assign
        Tournament tournament = new Tournament();
        tournament.Type = 1;
        //1 for tennis for now.
        tournament.Sport = 1;
        tournament.Details = new TournamentDetail
        {
            Sets = 1,
            NoSingles = 2,
            NoDoubles = 1,
            TeamSize = 2
        };

        tournament.Details.TeamSize = 2;
        tournament.Start = DateTime.Now;
        tournament.End = DateTime.Now;
        tournament.Interval = 1;
        tournament.Label = "Wicks Open";

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();
        Team? currentTeam = null;

        for (int i = 0; i < noPlayers; i++)
        {
            if (i % tournament.Details.TeamSize == 0)
            {
                currentTeam = new Team(i + 1, $"team_{i + 1}");
                teams.Add(currentTeam);
            }
            
            currentTeam?.AddPlayer(new Player
            {
                Id = i + 1,
                First = $"player_{i}",
                Last = $""
            });

        }

        //Action
        //Assert
        FactoryDrawMaker fac = new();
        var mm = fac.Create(tournament, gm);
        var results = mm.Create(teams);

        FileStream fs = new FileStream($"{tournament.Label}.pdf", FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new(results, new PDFTemplateFactory());
        printer.Print(fs, tournament, results, 0);
        
        
    }


}