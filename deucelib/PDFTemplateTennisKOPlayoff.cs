using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using deuce.ext;

namespace deuce;

/// <summary>
/// PDF template for generating Tennis Knockout (KO) Playoff tournament brackets.
/// Handles the layout and rendering of playoff rounds, main tournament rounds, 
/// and final matches on separate pages using iText PDF library.
/// </summary>
public class PDFTemplateTennisKOPlayoff : PDFTemplateBase
{
    // Page margin settings in points
    private float _page_top_margin = 10f;      // Top margin for pages
    private float _page_bottom_margin = 10f;   // Bottom margin for pages
    private float _page_left_margin = 10f;     // Left margin for pages
    private float _page_right_margin = 10f;    // Right margin for pages

    // Table cell padding settings in points
    private float _table_padding_top = 5f;     // Top padding for table cells
    private float _table_padding_bottom = 5f;  // Bottom padding for table cells
    private float _table_padding_left = 5f;    // Left padding for table cells
    private float _table_padding_right = 5f;   // Right padding for table cells

    /// <summary>
    /// Layout manager responsible for positioning tournament elements on pages
    /// </summary>
    public override ILayoutManager LayoutManager { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the PDFTemplateTennisKOPlayoff class.
    /// Sets up the layout manager with A4 landscape orientation and configured margins/padding.
    /// </summary>
    public PDFTemplateTennisKOPlayoff()
    {
        // Initialize the layout manager with landscape A4 page size and margin/padding settings
        LayoutManager = new LayoutManagerTennisKOPlayoff(
            PageSize.A4.Rotate().GetWidth(),
            PageSize.A4.Rotate().GetHeight(),
            _page_top_margin, _page_left_margin, _page_right_margin, _page_bottom_margin,
            _table_padding_top, _table_padding_bottom, _table_padding_left, _table_padding_right
        );
    }

    /// <summary>
    /// Generates the PDF document for a knockout playoff tournament.
    /// Creates separate pages for main rounds, playoff rounds, and final matches.
    /// </summary>
    /// <param name="doc">The iText document to add content to</param>
    /// <param name="pdfdoc">The PDF document being generated</param>
    /// <param name="tournament">The tournament data containing draws and matches</param>
    /// <param name="roundNo">The round number (currently unused in this implementation)</param>
    /// <param name="scores">Optional list of scores to display in the matches</param>
    /// <exception cref="ArgumentException">Thrown when tournament.Draw is null</exception>
    public override void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        // Validate that the tournament has a draw
        Draw s = tournament.Draw ?? throw new ArgumentException("Schedule cannot be null for PDF generation.");

        // Set the PDF to use A4 landscape orientation
        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        // Get the layout arrangement from the layout manager
        List<PagenationInfo> layout = (LayoutManager.ArrangeLayout(tournament) as List<PagenationInfo>) ?? new();

        // Configure table column widths: first column (team names) is wider, score columns are equal width
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details.Sets + 1); c++) widths.Add(c == 0 ? 2f : 1f);

        // PHASE 1: Print main tournament rounds (non-playoff, non-final matches)
        // Group layout items by page for main rounds
        var groupedLayout = from p in layout
                            where !p.IsPlayoffRound && !p.IsFinalMatch
                            group p by p.PageIndex into g
                            select new { PageIndex = g.Key, Layouts = g.ToList() };
        
        foreach (var group in groupedLayout)
        {
            // Add a new page for each group of main round matches
            pdfdoc.AddNewPage();
            // Print all matches assigned to this page
            PrintPage(group.Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, group.PageIndex);
        }

        // PHASE 2: Print playoff rounds (playoff matches, but not the final)
        var groupedLayoutPlayoff = from p in layout
                                   where p.IsPlayoffRound && !p.IsFinalMatch
                                   group p by p.PageIndex into g
                                   select new { PageIndex = g.Key, Layouts = g.ToList() };
        
        foreach (var group in groupedLayoutPlayoff)
        {
            // Add a new page for each group of playoff round matches
            pdfdoc.AddNewPage();
            // Print all matches assigned to this page
            PrintPage(group.Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, group.PageIndex);
        }

        // PHASE 3: Print the final match on its own page
        var groupFinalMatch = from p in layout
                                   where p.IsFinalMatch
                                   group p by p.PageIndex into g
                                   select new { PageIndex = g.Key, Layouts = g.ToList() };
        
        // Only print final match page if there is a final match to display                           
        if (groupFinalMatch.FirstOrDefault() != null)
        {
            pdfdoc.AddNewPage();
            PrintPage(groupFinalMatch.First().Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, groupFinalMatch.First().PageIndex);
        }
    }

    /// <summary>
    /// Prints a single page of the PDF with the specified layout elements.
    /// Handles different element types: round headers, round labels, and match tables.
    /// </summary>
    /// <param name="layout">List of pagination elements to render on this page</param>
    /// <param name="draw">The tournament draw containing round and match data</param>
    /// <param name="tournament">The tournament data for settings like number of sets</param>
    /// <param name="scores">List of scores to display in match tables</param>
    /// <param name="pdfdoc">The PDF document being generated</param>
    /// <param name="doc">The iText document for adding content</param>
    /// <param name="widths">Column width ratios for match tables</param>
    /// <param name="pageNo">The page number for positioning elements</param>
    private void PrintPage(List<PagenationInfo> layout, Draw draw, Tournament tournament, List<Score> scores, PdfDocument pdfdoc, Document doc,
    List<float> widths, int pageNo)
    {
        // Exit early if there are no elements to print on this page
        if (layout.Count == 0) return;

        // Exit early if there are no elements to print on this page
        if (layout.Count == 0) return;

        // Process each layout element on the page
        for (int i = 0; i < layout.Count; i++)
        {
            try
            {
                PagenationInfo pi = layout[i];
                
                // Handle different types of page elements differently
                if (pi.ElementType == PageElementType.RoundHeader)
                {
                    // Create and position a round header (e.g., "Round 1", "Quarterfinals")
                    float headerFontSize = 14f; // Fixed font size for headers
                    Paragraph headerParagraph = new Paragraph(pi.Text)
                        .SetFontSize(headerFontSize)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    
                    // Position the header at the specified coordinates
                    headerParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(headerParagraph);
                }
                else if (pi.ElementType == PageElementType.RoundLabel)
                {
                    // Create and position a round label (e.g., "Main Draw", "Playoff")
                    float labelFontSize = 16f; // Slightly larger font size for labels
                    Paragraph labelParagraph = new Paragraph(pi.Text)
                        .SetFontSize(labelFontSize)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    
                    // Position the label at the specified coordinates
                    labelParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(labelParagraph);
                }
                else // Handle match elements (tables showing teams and scores)
                {
                    // Find the specific round (either main draw or playoff) that contains this match
                    var round = pi.IsPlayoffRound ? draw.Rounds.FirstOrDefault(e => e.Index == pi.Round)?.Playoff :
                             draw.Rounds.FirstOrDefault(e => e.Index == pi.Round);

                    // Find the specific match within the round using the row offset (permutation ID)
                    Match? match = round?.Permutations.FirstOrDefault(p => p.Id == pi.RowOffset)?.Matches.FirstOrDefault(); 

                    // Create a table to display the match with teams and scores
                    // Each match has multiple sets, so we need columns for team names + score columns
                    Table matchTable = new Table(widths.ToArray());
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(pi.Rectangle.Width);
                    matchTable.SetHeight(pi.Rectangle.Height);
                    
                    // Position the table at the exact coordinates specified in the layout
                    matchTable.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);

                    // Calculate appropriate font size based on table height for readability
                    float fontSizePx = pi.Rectangle.Height / 4.2f; // Divide by 4.2 to ensure text fits well
                    float fontSizePt = fontSizePx * 72f / 96f;     // Convert from pixels to points
                    
                    // ROW 1: Home team and their scores for each set
                    var homeText = match?.Home?.FirstOrDefault()?.Team?.GetPlayerCSV() ?? "";
                    Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));
                    matchTable.AddCell(homeTeamCell);
                    
                    // Add score cells for each set of the home team
                    for (int j = 0; j < tournament.Details.Sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => x.Match == match?.Id && x.Set == j + 1)?.Home.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }

                    // ROW 2: Away team and their scores for each set
                    var awayText = match?.Away?.FirstOrDefault()?.Team?.GetPlayerCSV();
                    Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
                    matchTable.AddCell(awayTeamCell);
                    
                    // Add score cells for each set of the away team
                    for (int j = 0; j < tournament.Details.Sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => x.Match == match?.Id && x.Set == j + 1)?.Away.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }
                    
                    // Add the completed table to the document
                    doc.Add(matchTable);
                }
            }
            catch (Exception ex)
            {
                // Log any errors that occur while processing individual layout elements
                // Continue with the next element rather than failing the entire page
                Console.WriteLine($"Error printing match {i + 1} on page {pageNo}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Creates a specialized table cell for displaying scores with fixed dimensions.
    /// Uses a custom renderer to ensure consistent sizing regardless of content.
    /// </summary>
    /// <param name="padding">Padding around the cell content in points</param>
    /// <param name="text">The score text to display in the cell</param>
    /// <param name="fontSZizePt">Font size in points for the score text</param>
    /// <returns>A configured Cell with fixed dimensions and custom rendering</returns>
    private Cell MakeScoreCell(float padding, string? text = "", float fontSZizePt = 16f)
    {
        var scell = new Cell();
        
        // Calculate cell dimensions based on font size and padding
        // Use consistent sizing for all score cells regardless of actual content length
        float charWidth = fontSZizePt * 0.6f;                    // Estimate: 0.6em per character width
        float cellWidth = charWidth + 2 * padding;               // Add horizontal padding
        float cellHeight = fontSZizePt * 1.6f + 2 * padding;    // 1.6em line height + vertical padding

        // Apply custom renderer for fixed-size cells with score-specific formatting
        scell.SetNextRenderer(new FixedSizeCellRenderer(scell, text, cellWidth, cellHeight, 1f));
        scell.SetFontSize(fontSZizePt);
        
        return scell;
    }

}
