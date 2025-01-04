namespace deuce_unit;

using System.Data;
using System.Diagnostics;
using deuce;
using deuce.lib;
using MySql.Data.MySqlClient;



[TestClass]
public class UnitTestsPlayer
{

     [TestMethod]
     [DataRow(100)]
    public void set_n_players_returns_nothing(int noPlayers)
    {
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();
        MySqlCommand cmd = conn.CreateCommand();
        cmd.CommandText = "sp_set_player";
        cmd.CommandType = System.Data.CommandType.StoredProcedure;

        Random random = new Random();

        cmd.Parameters.AddWithValue("p_id", DBNull.Value);
        cmd.Parameters.AddWithValue("p_organization", 1);
        cmd.Parameters.AddWithValue("p_first_name", "");
        cmd.Parameters.AddWithValue("p_last_name", "");
        cmd.Parameters.AddWithValue("p_utr", 0d);

        for (int i = 0; i < noPlayers && i < RandomUtil.GetNameCount(); i++)
        {
            string[] rname = RandomUtil.GetNameAtIndex(i).Split(new char[] { ' ' });

            cmd.Parameters["p_organization"].Value = 1;
            cmd.Parameters["p_first_name"].Value = rname[0];
            cmd.Parameters["p_last_name"].Value = rname[1];
            cmd.Parameters["p_utr"].Value = random.NextDouble() * 10d;
            cmd.ExecuteNonQuery();
        }

        cmd.Dispose();
        conn.Close();

    }

    [TestMethod]
    [DataRow(1)]
    public async Task get_players_for_club_returns_list(int clubId)
    {
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");

        Organization c = new Organization(){ Id =  clubId};
        List<Player>? list = null;
        try {
            conn.Open();

            var fac = FactoryCreateDbRepo.Create<Player>(conn!, c);
            Filter filter = new(){ ClubId = c.Id};
             list = await fac!.GetList(filter)??new List<Player>();

             foreach(Player p in list) Debug.Write($"{p.Id}|{p.First}|{p.Last}\n");


        }
        catch(Exception ex) {

            Debug.WriteLine(ex.ToString());
            Assert.Fail(ex.ToString());
        }  

        conn.Close();

        //Asserts
        Assert.IsNotNull(list,"No players");
        Assert.IsTrue(list?.Count>1,"No players");
        
    }
}