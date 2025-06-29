namespace deuce_unit;
using deuce;
using MySql.Data.MySqlClient;
using System.Diagnostics;

[TestClass]
public class UnitTestsSettings
{
    public UnitTestsSettings() { }

    [TestMethod]
    public async Task get_list()
    {
        DbConnectionLocal conn = new("Server=localhost;Database=deuce;User Id=deuce;Password=deuce;");
        conn.Open();

        DbRepoSettings dbrepo = new(conn);
        List<Setting> listOfSettings = await dbrepo.GetList();
        conn.Close();

        foreach(Setting  setting in listOfSettings) Debug.WriteLine($"{setting.id}|{setting.key}|{setting.value}");
        
        Assert.IsNotNull(listOfSettings, "settings is null");
        Assert.IsTrue(listOfSettings.Count>0, "no settings");

    }
}
