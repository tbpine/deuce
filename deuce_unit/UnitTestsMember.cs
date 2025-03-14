namespace deuce_unit;

using System.Data;
using System.Diagnostics;
using deuce;
using MySql.Data.MySqlClient;



[TestClass]
public class UnitTestsMember
{

    [TestMethod]
    [DataRow(10,36)]
    public void set_n_members_returns_nothing(int noPlayers, int countryCode)
    {
        //Assign
        //Action
        //Assert
        
        DbConnectionLocal conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();
        //For UTR
        Random random = new Random();
        //Keep for assertions
        List<Member> newMembers = new();
        //Tournament DTO

        for (int i = 0; i < noPlayers && i < RandomUtil.GetNameCount(); i++)
        {
            //create the new player
            DbRepoMember depoMember = new(conn);
            
            string[] rname = RandomUtil.GetPlayer().Split(new char[] { ' ' });
            Member newMember = new()
            {
                Id = 0,
                First = rname[0],
                Last = rname[1],
                Middle = rname[1],
                Utr = random.NextDouble() * 10d,
                CountryCode = countryCode
                
            };
            newMembers.Add(newMember);
            //Save to db
            depoMember.Set(newMember);

        }

        foreach(Member member in newMembers) Assert.IsTrue(member.Id>0, "Member did not save");

    }

    [TestMethod]
    [DataRow(36)]
    public async Task get_players_for_country_returns_list(int countryCode)
    {
        DbConnectionLocal conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");

        List<Member>? list = null;
        try
        {
            conn.Open();

            var fac = new DbRepoMember(conn);
            Filter filter = new(){ CountryCode = countryCode };
            list = await fac!.GetList(filter) ?? new List<Member>();

            foreach (Member m in list) Debug.Write($"{m.Id}|{m.First}|{m.Last}\n");


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