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
        tournament.Type = new TournamentType(1, "Round Robbin");
        //1 for tennis for now.
        tournament.Sport = 1;
        tournament.Format = new Format(1, 1, 1);
        tournament.TeamSize = 1;

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();

        for (int i = 0; i < noPlayers; i++)
        {
            Team t = new Team(i + 1, $"team_{i + 1}");

            t.AddPlayer(new Player
            {
                Id = i + 1,
                First = $"player_{i}",
                Last = $""
            });

            teams.Add(t);

        }

        //Action
        //Assert
        FactorySchedulers fac = new();
        var mm = fac.Create(tournament, gm);
        var results = mm.Run(teams);

        FileStream fs = new FileStream(@"c:\\temp\\hello.pdf", FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new(results);
        printer.Print(fs, tournament, results, 1);
        
        
    }


}