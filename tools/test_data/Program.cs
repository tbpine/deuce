using System.Configuration;
using System.Globalization;
using MySql.Data.MySqlClient;

string connectionString = "Server=localhost;Database=deuce;User Id=deuce;Password=deuce;";


MySqlConnection dbconn = new MySqlConnection(connectionString);
dbconn.Open();

string[] names = { 
"John","Doe","Jane","Smith","Michael","Johnson","Emily","Davis","David","Brown","Sarah","Wilson","James","Taylor","Jessica","Martinez","Daniel","Anderson","Laura","Thomas","Robert","Jackson","Linda","White","Christopher","Harris","Patricia","Martin","Matthew","Thompson","Barbara","Garcia","Joshua","Martinez","Elizabeth","Robinson","Andrew","Clark","Susan","Lewis","Joseph","Walker","Karen","Hall","Thomas","Allen","Nancy","Young","Charles","King","Betty","Wright","Mark","Scott","Sandra","Green","Paul","Adams","Ashley","Baker","Steven","Nelson","Kimberly","Carter","Kevin","Mitchell","Donna","Perez","Brian","Roberts","Carol","Turner","George","Phillips","Michelle","Campbell","Edward","Parker","Amanda","Evans","Ronald","Edwards","Melissa","Collins","Anthony","Stewart","Deborah","Sanchez","Jason","Morris","Rebecca","Rogers","Jeffrey","Reed","Laura","Cook","Ryan","Morgan","Sharon","Bell","Gary","Murphy","Cynthia","Bailey","Nicholas","Rivera","Kathleen","Cooper","Eric","Richardson","Amy","Cox","Stephen","Howard","Angela","Ward","Jonathan","Torres","Brenda","Peterson","Timothy","Gray","Pamela","Ramirez","Larry","James","Katherine","Brooks","Scott","Kelly","Nicole","Sanders","Frank","Price","Christina","Bennett","Justin","Wood"
};
StreamWriter sw = new("data.sql");
for (int i = 0; i < names.Length-1; i+=2)  
{
    string first = names[i];
    string second = names[i+1];
    Random r = new Random((int)DateTime.Now.Ticks);
    double rank = (r.Next() % 10) + r.NextDouble();
    sw.Write($"call sp_set_player(NULL, '{first}','{second}', {rank.ToString("F2")});\n");

}

sw.Close();
dbconn.Close();