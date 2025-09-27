using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using deuce.ext;
using System.Diagnostics;
using DocumentFormat.OpenXml.Office.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Drawing;
using iText.Layout.Borders;

namespace deuce;

/// <summary>
/// PDF template for generating group tournament formats.
/// This class uses the LayoutManagerGroup to arrange matches by groups,
/// displaying each group's matches in a structured layout with proper pagination.
/// </summary>
public class PDFTemplateGroup : IPDFTemplate
{
    private float _page_top_margin = 10f;
    private float _page_bottom_margin = 10f;
    private float _page_left_margin = 10f;
    private float _page_right_margin = 10f;

    private float _table_padding_top = 5f;
    private float _table_padding_bottom = 5f;
    private float _table_padding_left = 5f;
    private float _table_padding_right = 5f;

    public ILayoutManager LayoutManager { get; private set; }

    public PDFTemplateGroup()
    {
        LayoutManager = new LayoutManagerGroup(
            PageSize.A4.Rotate().GetWidth(),
            PageSize.A4.Rotate().GetHeight(),
            _page_top_margin, _page_left_margin, _page_right_margin, _page_bottom_margin,
            _table_padding_top, _table_padding_bottom, _table_padding_left, _table_padding_right
        );
    }

    /// <summary>
    /// Generates a PDF document for a group tournament format.
    /// This method arranges matches by groups, displaying each group's matches
    /// in a structured layout with proper headers and group identification.
    /// The layout is determined by the provided tournament and its groups.
    /// </summary>
    /// <param name="doc">The iText Document object for layout and styling.</param>
    /// <param name="pdfdoc">The PDF document to generate.</param>
    /// <param name="tournament">The tournament object containing groups and match details.</param>
    /// <param name="roundNo">The round number to generate the PDF for. (not used in group format)</param>
    /// <param name="scores">Optional list of scores for the matches.</param>
    /// <exception cref="ArgumentException">Thrown when tournament or groups are null/empty.</exception>
    public void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
        List<Score>? scores = null)
    {
        // Validate input
        if (tournament?.Groups == null || !tournament.Groups.Any())
        {
            throw new ArgumentException("Tournament must have groups for group format PDF generation.");
        }

        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        List<PagenationInfo> layout = (LayoutManager.ArrangeLayout(tournament) as List<PagenationInfo>) ?? new();

        // Table widths for each match
        // The first column is wider for team names, the rest are equal for scores
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details?.Sets ?? 1) + 1; c++) 
            widths.Add(c == 0 ? 2f : 1f);

        // Group layout by page index using LINQ
        var groupedLayout = from p in layout
                           group p by p.PageIndex into g
                           select new { PageIndex = g.Key, Layouts = g.ToList() };

        foreach (var group in groupedLayout)
        {
            // Add a new page for each group
            pdfdoc.AddNewPage();
            
            // Print the matches on this page
            PrintPage(group.Layouts, tournament, scores ?? new(), pdfdoc, doc, widths, group.PageIndex);
        }
    }

    /// <summary>
    /// Prints the matches and headers for a specific page of the PDF document.
    /// This method handles group labels, round headers, and match tables.
    /// It iterates through the layout information, creating appropriate elements
    /// for each type (group label, round header, or match).
    /// </summary>
    /// <param name="layout">The layout information containing rectangles and match indices.</param>
    /// <param name="tournament">The tournament object containing group and match details.</param>
    /// <param name="scores">The list of scores for the matches.</param>
    /// <param name="pdfdoc">The PDF document to add the elements to.</param>
    /// <param name="doc">The document object for layout and styling.</param>
    /// <param name="widths">The column widths for the match tables.</param>
    /// <param name="pageNo">The page number for the current layout.</param>
    private void PrintPage(List<PagenationInfo> layout, Tournament tournament, List<Score> scores, 
        PdfDocument pdfdoc, Document doc, List<float> widths, int pageNo)
    {
        if (layout.Count == 0) return;

        for (int i = 0; i < layout.Count; i++)
        {
            try
            {
                PagenationInfo pi = layout[i];
                
                // Handle different element types
                if (pi.ElementType == PageElementType.RoundLabel)
                {
                    // Create a label for the group
                    float labelFontSize = 18f; // Larger font size for group labels
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    Paragraph labelParagraph = new Paragraph(pi.Text)
                        .SetFont(boldFont)
                        .SetFontSize(labelFontSize)
                        .SetTextAlignment(TextAlignment.CENTER);
                    
                    labelParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(labelParagraph);
                }
                else if (pi.ElementType == PageElementType.RoundHeader)
                {
                    // Create a header for the round within the group
                    float headerFontSize = 14f;
                    PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    Paragraph headerParagraph = new Paragraph(pi.Text)
                        .SetFont(boldFont)
                        .SetFontSize(headerFontSize)
                        .SetTextAlignment(TextAlignment.CENTER);
                    
                    headerParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(headerParagraph);
                }
                else // Handle match elements
                {
                    // Get the group and its draw for this layout item
                    Group? currentGroup = pi.Group;
                    if (currentGroup?.Draw == null) continue;

                    // Find the match in the group's draw
                    Match? match = currentGroup.Draw.Rounds
                        .FirstOrDefault(e => e.Index == pi.Round)?
                        .GetAtIndex(pi.RowOffset)?
                        .Matches.FirstOrDefault();

                    if (match == null) continue;

                    // Create match table
                    Table matchTable = new Table(widths.ToArray());
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(pi.Rectangle.Width);
                    matchTable.SetHeight(pi.Rectangle.Height);
                    matchTable.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);

                    // Calculate appropriate font size
                    float fontSizePx = pi.Rectangle.Height / 4.2f;
                    float fontSizePt = fontSizePx * 72f / 96f;

                    // Add home team cell
                    var homeText = match.Home?.FirstOrDefault()?.Team?.GetPlayerCSV() ?? "TBD";
                    Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));
                    matchTable.AddCell(homeTeamCell);

                    // Add home team score cells
                    int sets = tournament.Details?.Sets ?? 1;
                    for (int j = 0; j < sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => 
                            x.Match == match.Id && x.Set == j + 1)?.Home.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }

                    // Add away team cell
                    var awayText = match.Away?.FirstOrDefault()?.Team?.GetPlayerCSV() ?? "TBD";
                    Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
                    matchTable.AddCell(awayTeamCell);

                    // Add away team score cells
                    for (int j = 0; j < sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => 
                            x.Match == match.Id && x.Set == j + 1)?.Away.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }

                    doc.Add(matchTable);
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue with the next element
                Console.WriteLine($"Error printing element {i + 1} on page {pageNo}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Creates a cell with bordered styling for score display.
    /// This creates a fixed-size cell that can display score values
    /// with appropriate padding and border styling.
    /// </summary>
    /// <param name="padding">Size of margin around the score box container</param>
    /// <param name="text">Optionally, text to display in the cell</param>
    /// <param name="fontSizePt">Font size in points</param>
    /// <returns>A styled Cell for score display</returns>
    private Cell MakeScoreCell(float padding, string? text = "", float fontSizePt = 16f)
    {
        var scell = new Cell();
        
        // Calculate appropriate cell dimensions
        float charWidth = fontSizePt * 0.6f; // rough estimate: 0.6em per char
        float cellWidth = charWidth + 2 * padding;
        float cellHeight = fontSizePt * 1.6f + 2 * padding; // 1.6em for height

        // Use FixedSizeCellRenderer for consistent sizing
        scell.SetNextRenderer(new FixedSizeCellRenderer(scell, text, cellWidth, cellHeight, 1f));
        scell.SetFontSize(fontSizePt);
        
        return scell;
    }
}