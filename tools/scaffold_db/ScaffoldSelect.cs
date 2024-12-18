using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Xml;
using deuce.extensions;
using Org.BouncyCastle.Bcpg;

class ScaffoldSelect
{
    private StreamWriter? _sw;
    public ScaffoldSelect()
    {
    }

    public void MakeSql(string filename, bool dropOnly = false)
    {
        try
        {
            //open output
            _sw = new StreamWriter(filename);
            //load xml schema files.
            XmlDocument docTbl = new XmlDocument();
            XmlDocument docCols = new XmlDocument();

            docTbl.Load("tables.xml");
            docCols.Load("cols.xml");

            var nlTbls = docTbl.SelectNodes("//Tables")!;

            _sw.Write("DELIMITER //\n\n\n");

            foreach (XmlElement eTbl in nlTbls)
            {
                MakeSelects(eTbl, docCols, dropOnly);
                MakeInserts(eTbl, docCols, dropOnly);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        _sw?.Write("DELIMITER ;\n\n\n");
        _sw?.Close();

    }

    private void MakeSelects(XmlElement eTbl, XmlDocument docCols, bool dropOnly)
    {
        string tblName = eTbl.SelectSingleNode("TABLE_NAME")?.InnerText ?? "";
        //Columns
        _sw?.Write($"DROP PROCEDURE IF EXISTS `sp_get_{tblName}`//\n\n");
        if (dropOnly) return;

        _sw?.Write($"CREATE PROCEDURE `sp_get_{tblName}`(\n");
        _sw?.Write($")\nBEGIN\n\n");

        var nlCols = docCols.SelectNodes($"//Columns[TABLE_NAME='{tblName}']")!;
        StringBuilder sbCols = new();

        _sw?.Write("\tSELECT ");
        string primaryCol = "";
        foreach (XmlElement eCol in nlCols)
        {
            string colName = eCol.SelectSingleNode("COLUMN_NAME")?.InnerText!;
            string colKey = eCol.SelectSingleNode("COLUMN_KEY")?.InnerText!;
            sbCols.Append("`" + colName + "`,");
            if (String.Compare(colKey, "PRI", true) == 0) primaryCol = colName;

        }
        sbCols.TrimRight(1);
        _sw?.Write(sbCols.ToString());
        _sw?.Write($"\n\tFROM `{tblName}`");
        if (!String.IsNullOrEmpty(primaryCol))
            _sw?.Write($"\n\tORDER BY `{primaryCol}`;\n\n\n END//\n\n\n");
        else
            _sw?.Write(";\n\n\n END//\n\n\n");

    }
    private void MakeInserts(XmlElement eTbl, XmlDocument docCols, bool dropOnly)
    {
        string tblName = eTbl.SelectSingleNode("TABLE_NAME")?.InnerText ?? "";
        //Columns
        _sw?.Write($"DROP PROCEDURE IF EXISTS `sp_set_{tblName}`//\n\n");
        if (dropOnly) return;

        _sw?.Write($"CREATE PROCEDURE `sp_set_{tblName}`(\n");
        
        var nlCols = docCols.SelectNodes($"//Columns[TABLE_NAME='{tblName}']")!;
        StringBuilder sbParms = new();
        StringBuilder sbValues = new();
        StringBuilder sbCols = new();
        StringBuilder sbSet = new();
        string[] excList = new string[] { "id" , "created_datetime"};
        string[] excParms = new string[] { "updated_datetime" , "created_datetime"};
        string[] excSet = new string[] { "id", "created_datetime", "updated_datetime"};

        foreach (XmlElement eCol in nlCols)
        {
            string colName = eCol.SelectSingleNode("COLUMN_NAME")?.InnerText!;
            string colType = eCol.SelectSingleNode("COLUMN_TYPE")?.InnerText!;

            if (Array.Find(excParms, e=> e == colName) == null)
                sbParms.Append($"IN p_{colName} {colType.ToUpper()},\n");

            sbCols.Append("`" + colName + "`,");
            sbValues.Append(" " + GetValue(colName) + ",");
            if (colName == "updated_datetime")
                sbSet.Append($"`{colName}` = NOW(),");
            else if (Array.Find(excList, e=> e == colName) == null)
                sbSet.Append($"`{colName}` = p_{colName},");

        }
        sbCols.TrimRight(1);
        sbParms.TrimRight(2);
        sbValues.TrimRight(1);
        sbValues.TrimLeft(1);
        sbSet.TrimRight(1);

        _sw?.Write(sbParms.ToString());
        _sw?.Write(")\n\nBEGIN\n\n");
        _sw?.Write($"INSERT INTO `{tblName}`({sbCols}) VALUES ({sbValues})\n");
        _sw?.Write($"ON DUPLICATE KEY UPDATE {sbSet};\n\n");

        _sw?.Write($"SELECT LAST_INSERT_ID() 'id';\n\n");
        _sw?.Write($"END//\n\n");
    }

    private string GetValue(string colName)
    {
        switch (colName)
        {
            case "created_datetime":
            case "updated_datetime":
                    return "NOW()";
            default:
                return $"p_{colName}";
        }
    }
}