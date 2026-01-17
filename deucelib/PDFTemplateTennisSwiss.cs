using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using deuce.ext;

namespace deuce;

/// <summary>
/// A class containing methods to layout elements in a PDF document
/// for Swiss System tennis tournaments.
/// Swiss format pairs teams with similar scores in each round.
/// </summary>
public class PDFTemplateTennisSwiss : PDFTemplateBase
{
    public override ILayoutManager LayoutManager { get; protected set; }

    /// <summary>
    /// Constructor for Swiss format PDF template
    /// </summary>
    public PDFTemplateTennisSwiss()
    {
        // Swiss format uses standard A4 layout with Swiss-specific layout manager
        LayoutManager = new LayoutManagerSwiss(
            595f, 842f, // A4 size
            36f, 36f, 36f, 36f, // Margins
            5f, 5f, // Padding
            5f, 5f // Table padding
        );
    }

    /// <summary>
    /// Generates a PDF showing matches for a specific round in a Swiss tournament.
    /// This method only displays match pairings and score recording areas.
    /// Standings should be generated separately using the GenerateStandings method.
    /// In Swiss format, teams are paired based on current standings/points.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="roundNo">The round to print</param>
    /// <param name="scores">Optionally, a list of scores to print</param>
    public override void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
        List<Score>? scores = null)
    {
        Draw s = tournament.Draw ?? throw new ArgumentNullException(nameof(tournament.Draw), "Tournament schedule cannot be null");

        MakeHeader(doc, tournament, roundNo);

        // Add matches header
        var matchesHeader = new Paragraph($"Round {roundNo} Matches")
            .SetFontSize(14)
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(15);
        doc.Add(matchesHeader);

        // Get the specific round for Swiss format
        // roundNo is not zero-based but the schedule is
        IEnumerable<Permutation> perms = s.GetRound(roundNo-1).Permutations;

        PdfFont cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        foreach (Permutation perm in perms)
        {
            // Calculate how many boxes to print for scores:
            int noScores = tournament?.Details?.Sets ?? 1;

            var thome = perm.GetTeamAtIndex(0);
            var taway = perm.GetTeamAtIndex(1);

            // Print match pairing info
            Paragraph pvs = new Paragraph($"{thome.Label} vs {taway.Label}")
                .SetFontSize(11);
                
            // Team membership details
            Paragraph pteam1 = new Paragraph($"{thome.Label}: {thome.GetPlayerCSV()}")
                .SetFontSize(8)
                .SetMarginBottom(2);
            Paragraph pteam2 = new Paragraph($"{taway.Label}: {taway.GetPlayerCSV()}")
                .SetFontSize(8)
                .SetMarginBottom(10);

            doc.Add(pvs);
            doc.Add(pteam1);
            doc.Add(pteam2);

            // Create table for matches in this pairing
            CreateMatchTable(doc, perm, noScores, cellFont, roundNo, scores);
        }

        // Generate method should only show matches - standings are generated separately via GenerateStandings method
    }

    /// <summary>
    /// Creates a table for matches within a pairing (permutation)
    /// </summary>
    private void CreateMatchTable(Document doc, Permutation perm, int noScores, 
        PdfFont cellFont, int roundNo, List<Score>? scores)
    {
        // Dynamically create column widths
        // First column (player names) gets more space, score columns get equal space
        List<float> colWidths = [noScores * 1.5f]; // Player names column
        for (int i = 0; i < noScores; i++) colWidths.Add(1); // Score columns

        foreach (Match match in perm.Matches)
        {
            Table tbl = new(colWidths.ToArray());
            tbl.SetFixedLayout();
            tbl.SetWidth(UnitValue.CreatePercentValue(100));
            tbl.SetFont(cellFont);

            string homeName = match.GetHomeTeam();
            string awayName = match.GetAwayTeam();

            // Add match type header
            string matchType = match.IsDouble ? "Doubles" : "Singles";
            string header = $"{matchType}: {homeName} vs {awayName}";
            Cell headerCell = new Cell(1, colWidths.Count)
                .Add(new Paragraph(header))
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetPadding(5)
                .SetTextAlignment(TextAlignment.CENTER);
            tbl.AddHeaderCell(headerCell);

            // Add column headers
            Cell playerHeaderCell = new Cell().Add(new Paragraph("Players"))
                .SetBackgroundColor(ColorConstants.GRAY)
                .SetPadding(3);
            tbl.AddHeaderCell(playerHeaderCell);

            for (int i = 0; i < noScores; i++)
            {
                Cell setHeader = new Cell().Add(new Paragraph($"Set {i + 1}"))
                    .SetBackgroundColor(ColorConstants.GRAY)
                    .SetPadding(3)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE);
                tbl.AddHeaderCell(setHeader);
            }

            // Home team row
            var cellHome = new Cell().Add(new Paragraph(homeName))
                .SetPadding(8);
            tbl.AddCell(cellHome);

            for (int i = 0; i < noScores; i++)
            {
                var score = scores?.Find(x => x.Match == match.Id && x.Set == i + 1 && x.Round == roundNo-1 &&
                x.Permutation == perm.Id);
                var cellHomeScore = MakeScoreCell(3f);

                if (score != null)
                {
                    var scoreParagraph = new Paragraph(score.Home.ToString())
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetFontSize(14);
                        
                    cellHomeScore.Add(scoreParagraph);
                }

                tbl.AddCell(cellHomeScore);
            }

            // Away team row
            var cellAway = new Cell().Add(new Paragraph(awayName))
                .SetPadding(8);
            tbl.AddCell(cellAway);

            for (int i = 0; i < noScores; i++)
            {
                var score = scores?.Find(x => x.Match == match.Id && x.Set == i + 1 && x.Round == roundNo-1 &&
                x.Permutation == perm.Id);
                var cellAwayScore = MakeScoreCell(3f);

                if (score != null)
                {
                    var scoreParagraph = new Paragraph(score.Away.ToString())
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .SetFontSize(14);
                        
                    cellAwayScore.Add(scoreParagraph);
                }

                tbl.AddCell(cellAwayScore);
            }

            tbl.SetMarginBottom(15f);
            doc.Add(tbl);
        }
    }

    /// <summary>
    /// Groups permutations by point brackets for Swiss format display
    /// This helps organize matches by teams with similar standings
    /// </summary>
    private Dictionary<string, List<Permutation>> GroupPermutationsByPoints(
        IEnumerable<Permutation> perms, Tournament tournament, int roundNo, List<Score>? scores)
    {
        var grouped = new Dictionary<string, List<Permutation>>();

        // For first round, no grouping needed
        if (roundNo == 1)
        {
            grouped[""] = perms.ToList();
            return grouped;
        }

        // Group by approximate point ranges
        foreach (var perm in perms)
        {
            var team1 = perm.GetTeamAtIndex(0);
            var team2 = perm.GetTeamAtIndex(1);

            // Calculate approximate points for grouping (simplified)
            int team1Points = CalculateTeamPoints(team1, tournament, roundNo, scores);
            int team2Points = CalculateTeamPoints(team2, tournament, roundNo, scores);
            int avgPoints = (team1Points + team2Points) / 2;

            string bracket = $"Point Bracket: {avgPoints} points";
            
            if (!grouped.ContainsKey(bracket))
            {
                grouped[bracket] = new List<Permutation>();
            }
            
            grouped[bracket].Add(perm);
        }

        // If only one group, remove the bracket header
        if (grouped.Count == 1)
        {
            var onlyGroup = grouped.First();
            grouped.Clear();
            grouped[""] = onlyGroup.Value;
        }

        return grouped;
    }

    /// <summary>
    /// Calculate team points up to a specific round (simplified calculation)
    /// </summary>
    private int CalculateTeamPoints(Team team, Tournament tournament, int roundNo, List<Score>? scores)
    {
        if (scores == null || !scores.Any()) return 0;

        // Simple point calculation - in real implementation this would be more sophisticated
        return scores.Where(s => s.Round < roundNo)
                   .SelectMany(s => new[] { s.Home, s.Away })
                   .Sum(point => point);
    }

    /// <summary>
    /// Creates a cell with bordered rectangle for score entry
    /// </summary>
    /// <param name="padding">Padding around the score box</param>
    /// <param name="text">Optional text to display in the score box</param>
    private Cell MakeScoreCell(float padding, string? text = "")
    {
        var scell = new Cell();
        scell.SetNextRenderer(new FixedSizeCellRenderer(scell, text));
        scell.SetPadding(padding);
        scell.SetTextAlignment(TextAlignment.CENTER);
        scell.SetVerticalAlignment(VerticalAlignment.MIDDLE);
        
        return scell;
    }

    /// <summary>
    /// Generates a comprehensive PDF showing both the standings and matches for a specific round in a Swiss tournament.
    /// This method creates a complete tournament report including current standings and round match details.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="roundNo">The round for which to show standings and matches (1-based)</param>
    /// <param name="scores">Optional scores to display in the matches section</param>
    public void GenerateStandingsWithMatches(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo, List<Score>? scores = null)
    {
        // First, generate the standings for the previous round (if available)
        if (roundNo > 1)
        {
            GenerateStandings(doc, pdfdoc, tournament, roundNo - 1);
            
            // Add page break between standings and matches
            doc.Add(new AreaBreak(iText.Layout.Properties.AreaBreakType.NEXT_PAGE));
        }
        
        // Then generate the matches for the current round
        Generate(doc, pdfdoc, tournament, roundNo, scores);
        
        // If this is round 1, add standings after matches (initial standings)
        if (roundNo == 1)
        {
            // Add some spacing
            doc.Add(new Paragraph("\n").SetFontSize(20));
            
            // Add a separator
            var separator = new Paragraph("═".PadRight(80, '═'))
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(10)
                .SetMarginBottom(10);
            doc.Add(separator);
            
            GenerateStandings(doc, pdfdoc, tournament, roundNo);
        }
    }

    /// <summary>
    /// Generates a PDF showing the standings for a specific round in a Swiss tournament.
    /// This method creates a table displaying team rankings, wins, losses, draws, and points.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="roundNo">The round for which to show standings (1-based)</param>
    public override void GenerateStandings(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo)
    {
        // Get standings for the specified round (convert to 0-based)
        var standings = tournament.GetStandingsForRound(roundNo - 1);
        
        if (standings == null || !standings.Any())
        {
            // If no standings available, show message
            MakeStandingsHeader(doc, tournament, roundNo);
            
            var noStandingsMsg = new Paragraph("No standings available for this round.")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(50);
            doc.Add(noStandingsMsg);
            return;
        }

        MakeStandingsHeader(doc, tournament, roundNo);

        // Sort standings by points (descending), then by position
        var sortedStandings = standings
            .OrderByDescending(s => s.Points)
            .ThenBy(s => s.Position)
            .ToList();

        // Create standings table
        CreateStandingsTable(doc, sortedStandings, roundNo);

        // Add summary statistics
        AddStandingsSummary(doc, sortedStandings, roundNo);
    }

    /// <summary>
    /// Creates a header for the standings PDF
    /// </summary>
    private void MakeStandingsHeader(Document doc, Tournament tournament, int roundNo)
    {
        PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        PdfFont regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        
        string title = $"{tournament.Label} - Swiss System Standings";
        
        // Calculate the date for this round
        Interval interval = new Interval(tournament.Interval, "");
        DateTime roundDate = tournament.Start.AddDays(roundNo * Utils.GetNoDays(interval));
        
        string dates = $"Standings after Round {roundNo} - {roundDate:dd MMM yyyy}";
        string format = $"Format: {tournament.Details?.NoSingles ?? 1} singles, {tournament.Details?.NoDoubles ?? 1} doubles, sets: {tournament.Details?.Sets ?? 1}";

        // Tournament title
        var titleParagraph = new Paragraph(title)
            .SetFont(boldFont)
            .SetFontSize(16)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(5);
        doc.Add(titleParagraph);

        // Round and date info
        var datesParagraph = new Paragraph(dates)
            .SetFont(regularFont)
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(3);
        doc.Add(datesParagraph);
        
        // Format information
        var formatParagraph = new Paragraph(format)
            .SetFont(regularFont)
            .SetFontSize(10)
            .SetMarginBottom(20);
        doc.Add(formatParagraph);
    }

    /// <summary>
    /// Creates the standings table showing team positions, names, and statistics
    /// </summary>
    private void CreateStandingsTable(Document doc, List<TeamStanding> standings, int roundNo)
    {
        // Define column widths: Position, Team Name, Wins, Losses, Draws, Points
        float[] columnWidths = { 1f, 4f, 1.5f, 1.5f, 1.5f, 2f };
        
        Table standingsTable = new Table(columnWidths);
        standingsTable.SetWidth(UnitValue.CreatePercentValue(100));
        standingsTable.SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA));

        // Add header row
        AddStandingsHeaderRow(standingsTable);

        // Add team rows
        int position = 1;
        foreach (var standing in standings)
        {
            AddStandingsTeamRow(standingsTable, standing, position);
            position++;
        }

        doc.Add(standingsTable);
    }

    /// <summary>
    /// Adds the header row to the standings table
    /// </summary>
    private void AddStandingsHeaderRow(Table table)
    {
        PdfFont headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        var headers = new[] { "Pos", "Team", "Wins", "Losses", "Draws", "Points" };
        
        foreach (var header in headers)
        {
            Cell headerCell = new Cell()
                .Add(new Paragraph(header))
                .SetFont(headerFont)
                .SetBackgroundColor(ColorConstants.DARK_GRAY)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(8);
            
            table.AddHeaderCell(headerCell);
        }
    }

    /// <summary>
    /// Adds a team row to the standings table
    /// </summary>
    private void AddStandingsTeamRow(Table table, TeamStanding standing, int position)
    {
        PdfFont cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        
        // Alternate row colors for better readability
        Color backgroundColor = position % 2 == 0 ? ColorConstants.LIGHT_GRAY : ColorConstants.WHITE;

        // Position cell
        Cell posCell = new Cell()
            .Add(new Paragraph(position.ToString()))
            .SetFont(cellFont)
            .SetBackgroundColor(backgroundColor)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetPadding(6);
        table.AddCell(posCell);

        // Team name cell (with players if available)
        string teamInfo = standing.Team.Label;
        if (!string.IsNullOrEmpty(standing.Team.GetPlayerCSV()))
        {
            teamInfo += $"\n({standing.Team.GetPlayerCSV()})";
        }
        
        Cell teamCell = new Cell()
            .Add(new Paragraph(teamInfo))
            .SetFont(cellFont)
            .SetBackgroundColor(backgroundColor)
            .SetVerticalAlignment(VerticalAlignment.MIDDLE)
            .SetPadding(6);
        table.AddCell(teamCell);

        // Statistics cells
        var stats = new[] { 
            standing.Wins.ToString(), 
            standing.Losses.ToString(), 
            standing.Draws.ToString(), 
            standing.Points.ToString("F1") 
        };

        foreach (var stat in stats)
        {
            Cell statCell = new Cell()
                .Add(new Paragraph(stat))
                .SetFont(cellFont)
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetPadding(6);
            table.AddCell(statCell);
        }
    }

    /// <summary>
    /// Adds summary statistics below the standings table
    /// </summary>
    private void AddStandingsSummary(Document doc, List<TeamStanding> standings, int roundNo)
    {
        if (!standings.Any()) return;

        // Calculate summary statistics
        int totalTeams = standings.Count;
        double totalPoints = standings.Sum(s => s.Points);
        double averagePoints = totalPoints / totalTeams;
        var topScore = standings.Max(s => s.Points);
        int teamsWithTopScore = standings.Count(s => s.Points == topScore);

        // Create summary paragraph
        var summaryText = $"Tournament Summary after Round {roundNo}:\n" +
                         $"• Total Teams: {totalTeams}\n" +
                         $"• Average Points: {averagePoints:F1}\n" +
                         $"• Leading Score: {topScore:F1} points\n" +
                         $"• Teams tied for lead: {teamsWithTopScore}";

        if (teamsWithTopScore > 1)
        {
            var leadingTeams = standings.Where(s => s.Points == topScore).Select(s => s.Team.Label);
            summaryText += $"\n• Leading teams: {string.Join(", ", leadingTeams)}";
        }

        var summaryParagraph = new Paragraph(summaryText)
            .SetFontSize(10)
            .SetMarginTop(20)
            .SetPadding(10)
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetBorder(new SolidBorder(ColorConstants.GRAY, 1));

        doc.Add(summaryParagraph);
    }

    /// <summary>
    /// Creates the header for the Swiss format round
    /// </summary>
    private void MakeHeader(Document doc, Tournament t, int roundNo)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        string tname = $"{t.Label} - Swiss System";
        
        // Calculate the date for this round
        Interval interval = new Interval(t.Interval, "");
        DateTime roundDate = t.Start.AddDays(roundNo * Utils.GetNoDays(interval));
        
        string dates = $"{roundDate:dd MMM yyyy} - Round {roundNo}";
        string location = "Location:";
        string format = $"Format: {t!.Details?.NoSingles ?? 1} singles, {t.Details?.NoDoubles ?? 1} doubles, sets: {t.Details?.Sets ?? 1}";
        string swissInfo = "Swiss System: Teams paired by current standings";

        doc.SetFont(font);
        
        // Tournament title
        var titleParagraph = new Paragraph(tname)
            .SetFontSize(16)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(5);
        doc.Add(titleParagraph);

        // Round and date info
        doc.SetFontSize(12);
        doc.Add(new Paragraph(dates)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(3));
        
        // Location (to be filled in)
        doc.Add(new Paragraph(location)
            .SetFontSize(10)
            .SetMarginBottom(2));
        
        // Format information
        doc.Add(new Paragraph(format)
            .SetFontSize(10)
            .SetMarginBottom(2));
            
        // Swiss system explanation
        doc.Add(new Paragraph(swissInfo)
            .SetFontSize(9)
            .SetMarginBottom(15));
    }
}