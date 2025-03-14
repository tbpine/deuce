// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using MySql.Data.MySqlClient;

// var dbconn = new MySqlConnection("server=localhost;user=deuce;password=deuce;database=deuce");

using StreamReader sr = new StreamReader("all.csv");

StringBuilder sbSQl = new();
string? alltext = sr.ReadToEnd();
string[] lines = alltext.Split(new char[] { '\n' });


for (int i = 1; i < lines.Length; i++)  
{
    string line = lines[i];
    if (string.IsNullOrEmpty(line)) continue;
    bool inquote = false;
    List<string> fields = new();
    StringBuilder field = new();
    for (int j = 0; j < line.Length ;j++)
    {
        if (line[j] == '"')  inquote = !inquote;
        else if (line[j] == ',' && !inquote)
        {
            fields.Add(field.ToString());
            field.Clear();
        }
        else
            field.Append(line[j]);

    }
    //last field
    fields.Add(field.ToString());
    sbSQl.Append("INSERT INTO `iso_3166` VALUES (");
    for (int k =0 ;  k < fields.Count ; k++)   
    {
        string column = string.IsNullOrEmpty(fields[k]) ? "NULL" : fields[k];
        if (k == 0) sbSQl.Append($@"""{column}"",");
        else if (k == 1) sbSQl.Append($@"""{column}"",");
        else if (k == 2) sbSQl.Append($@"""{column}"",");
        else if (k == 3) sbSQl.Append($@"{column},");
        else if (k == 4) sbSQl.Append($@"""{column}"",");
        else if (k == 5) sbSQl.Append($@"""{column}"",");
        else if (k == 6) sbSQl.Append($@"""{column}"",");
        else if (k == 7) sbSQl.Append($@"""{column}"",");
        else if (k == 8) sbSQl.Append($@"{column},");
        else if (k == 9) sbSQl.Append($@"{column},");
        else if (k == 10) sbSQl.Append($@"{column}");
    }
    sbSQl.AppendLine(");");

}
using StreamWriter sw = new("iso3166.sql");
sw.Write(sbSQl.ToString());
