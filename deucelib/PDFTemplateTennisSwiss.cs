using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using deuce.ext;

namespace deuce;

/// <summary>
/// A class containing methods to layout elements in a PDF document
/// for Swiss System tennis tournaments.
/// Swiss format pairs teams with similar scores in each round.
/// </summary>
public class PDFTemplateTennisSwiss : IPDFTemplate
{
    public ILayoutManager LayoutManager { get; private set; }

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
    /// Prints details of matches in a Swiss round and provides space to record scores.
    /// In Swiss format, teams are paired based on current standings/points.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="roundNo">The round to print</param>
    /// <param name="scores">Optionally, a list of scores to print</param>
    public void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
        List<Score>? scores = null)
    {
        Draw s = tournament.Draw ?? throw new ArgumentNullException(nameof(tournament.Draw), "Tournament schedule cannot be null");

        MakeHeader(doc, tournament, roundNo);

        // Get the specific round for Swiss format
        IEnumerable<Permutation> perms = s.GetRounds(roundNo).Permutations;

        PdfFont cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Group matches by point brackets for better readability in Swiss format
        var groupedPerms = GroupPermutationsByPoints(perms, tournament, roundNo, scores);

        foreach (var group in groupedPerms)
        {
            // Add point bracket header if applicable
            if (!string.IsNullOrEmpty(group.Key))
            {
                var bracketHeader = new Paragraph(group.Key)
                    .SetFontSize(12)
                    .SetMarginTop(10)
                    .SetMarginBottom(5)
                    .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                    .SetPadding(5);
                doc.Add(bracketHeader);
            }

            foreach (Permutation perm in group.Value)
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
        }
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
                var score = scores?.Find(x => x.Match == match.Id && x.Set == i + 1 && x.Round == roundNo);
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
                var score = scores?.Find(x => x.Match == match.Id && x.Set == i + 1 && x.Round == roundNo);
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
        if (roundNo == 0)
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
    /// Creates the header for the Swiss format round
    /// </summary>
    private void MakeHeader(Document doc, Tournament t, int roundNo)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        string tname = $"{t.Label} - Swiss System";
        
        // Calculate the date for this round
        Interval interval = new Interval(t.Interval, "");
        DateTime roundDate = t.Start.AddDays(roundNo * Utils.GetNoDays(interval));
        
        string dates = $"{roundDate:dd MMM yyyy} - Round {roundNo + 1}";
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