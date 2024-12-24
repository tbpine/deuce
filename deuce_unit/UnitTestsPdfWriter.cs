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
        tournament.Format = new Format(2, 2, 1);
        tournament.TeamSize = 2;
        tournament.Start = DateTime.Now;
        tournament.End = DateTime.Now;
        tournament.Interval = new Interval(1, "Weekly");
        tournament.Label = "Wicks Open";

        IGameMaker gm = new GameMakerTennis();
        //Teams
        List<Team> teams = new();
        Team? currentTeam = null;

        for (int i = 0; i < noPlayers; i++)
        {
            if (i % tournament.TeamSize == 0)
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
        FactorySchedulers fac = new();
        var mm = fac.Create(tournament, gm);
        var results = mm.Run(teams);

        FileStream fs = new FileStream($"{tournament.Label}.pdf", FileMode.Create, FileAccess.Write);
        PdfPrinter printer = new(results);
        printer.Print(fs, tournament, results, 0);
        
        
    }


}