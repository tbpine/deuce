namespace deuce_unit;
using deuce.ext;
using deuce;
using MySql.Data.MySqlClient;
using System.Data.Common;
using deuce.lib;
using System.Diagnostics;

[TestClass]
public class UnitTestsSport
{
    public UnitTestsSport() { }

    [TestMethod]
    public async Task get_list()
    {
        MySqlConnection conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();

        DbRepoSport dbrepo = new(conn);
        List<Sport> sports = await dbrepo.GetList();
        conn.Close();

        foreach(Sport  sport in sports) Debug.WriteLine($"{sport.Id}|{sport.Name}");
        
        Assert.IsNotNull(sports, "sports is null");
        Assert.IsTrue(sports.Count>0, "missing sports");

    }
}
