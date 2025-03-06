namespace deuce_unit;

using System.Data;
using System.Diagnostics;
using deuce;
using MySql.Data.MySqlClient;



[TestClass]
public class UnitTestsPlayer
{

    [TestMethod]
    [DataRow(10)]
    public void set_n_players_returns_nothing(int noPlayers)
    {
        //Assign
        //Action
        //Assert
        
        DbConnectionLocal conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();
        Organization org = new Organization() { Id = 1, Name = "testing" };
        //For UTR
        Random random = new Random();
        //Keep for assertions
        List<Player> newPlayers = new();

        for (int i = 0; i < noPlayers && i < RandomUtil.GetNameCount(); i++)
        {
            //create the new player
            DbRepoPlayer depoPlayer = new(conn, org);
            string[] rname = RandomUtil.GetPlayer().Split(new char[] { ' ' });
            Player newPlayer = new()
            {
                Id = 0,
                First = rname[0],
                Last = rname[1],
                Ranking = random.NextDouble() * 10d,
            };
            newPlayers.Add(newPlayer);
            //Save to db
            depoPlayer.Set(newPlayer);

        }

        foreach(Player player in newPlayers) Assert.IsTrue(player.Id>0, "Player did not save");

    }

    [TestMethod]
    [DataRow(1)]
    public async Task get_players_for_club_returns_list(int clubId)
    {
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");

        Organization c = new Organization() { Id = clubId };
        List<Player>? list = null;
        try
        {
            conn.Open();

            var fac = new DbRepoPlayer(conn, c);
            Filter filter = new() { ClubId = c.Id };
            list = await fac!.GetList(filter) ?? new List<Player>();

            foreach (Player p in list) Debug.Write($"{p.Id}|{p.First}|{p.Last}\n");


        }
        catch (Exception ex)
        {

            Debug.WriteLine(ex.ToString());
            Assert.Fail(ex.ToString());
        }

        conn.Close();

        //Asserts
        Assert.IsNotNull(list, "No players");
        Assert.IsTrue(list?.Count > 1, "No players");

    }
}