using ClosedXML.Excel;

namespace deuce;

/// <summary>
/// Import teams using ClosedXML library.
/// </summary>
public class ImporterTeamsClosedXML : ImporterTeamsBase
{
    /// <summary>
    /// Imports teams from an Excel file using ClosedXML.
    /// </summary>
    /// <param name="stream">The source excel file in a stream</param>
    /// <returns>List of Teams</returns>
    public override List<Team> Load(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1); // Assuming the first worksheet contains the data

        var teams = new List<Team>();
        foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header row
        {
            var firstName = row.Cell(1).GetString();
            var middleName = row.Cell(2).GetString();
            var lastName = row.Cell(3).GetString();
            var teamName = row.Cell(4).GetString();

            var team = teams.FirstOrDefault(t => t.Label == teamName);
            if (team == null)
            {
                team = new Team();
                team.Label = teamName;
                teams.Add(team);
            }

            team.AddPlayer(new Player { First = firstName, Middle = middleName , Last = lastName });
        }

        return teams;
    }   

}