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

public class PDFTemplateTennisKO : IPDFTemplate
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

    public PDFTemplateTennisKO()
    {
        LayoutManager = new LayoutManagerTennisKO(
            PageSize.A4.Rotate().GetWidth(),
            PageSize.A4.Rotate().GetHeight(),
            _page_top_margin, _page_left_margin, _page_right_margin, _page_bottom_margin,
            _table_padding_top, _table_padding_bottom, _table_padding_left, _table_padding_right
        );
    }

    /// <summary>
    /// Generates a PDF document for a tennis knockout tournament.
    /// This method arranges matches in a knockout format,
    /// displaying home and away teams along with their scores.
    /// The layout is determined by the provided tournament schedule.
    /// The scores are optional and can be provided to display match results.
    /// </summary>
    /// <param name="doc"> </param>
    /// <param name="pdfdoc"></param>
    /// <param name="tournament">
    /// The tournament object containing details and schedule.
    /// <param name="roundNo">The round number to generate the PDF for. (not used in this implementation)
    /// <param name="scores"> Optional list of scores for the matches.
    /// <exception cref="ArgumentException"></exception>
    public void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        Draw s = tournament.Draw ?? throw new ArgumentException("Schedule cannot be null for PDF generation.");

        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        List<PagenationInfo> layout = (LayoutManager.ArrangeLayout(tournament) as List<PagenationInfo>) ?? new();

        // table widths for each match
        // The first column is wider for team names, the rest are equal for scores
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details.Sets + 1); c++) widths.Add(c == 0 ? 2f : 1f);
        //Store the page number
        //LINQ to group by page index
        var groupedLayout = from p in layout
                            group p by p.PageIndex into g
                            select new { PageIndex = g.Key, Layouts = g.ToList() };
        foreach (var group in groupedLayout)
        {
            //Add a new page for each group
            pdfdoc.AddNewPage();
            //Print the matches on this page
            //Find ind matches associated with this layout
            
            PrintPage(group.Layouts, s, tournament, scores??new(), pdfdoc, doc, widths, group.PageIndex);
        }


    }

    /// <summary>
    /// Prints the matches on a specific page of the PDF document.
    /// This method iterates through the layout information,    
    /// creating a table for each match.
    /// It retrieves match details, including home and away teams,
    /// and their scores.
    /// The table is styled with fixed positions and font sizes.
    /// It handles exceptions for each match to ensure that
    /// the PDF generation continues even if some matches have issues.
    /// </summary>
    /// <param name="layout"> The layout information containing match rectangles and indices.</param>
    /// <param name="s">The schedule object containing match details.</param>
    /// <param name="tournament">The tournament object containing tournament details.</param>
    /// <param name="scores">The list of scores for the matches.</param>
    /// <param name="pdfdoc">The PDF document to add the matches to.</param>
    /// <param name="doc">The document object for layout and styling.</param>
    /// <param name="widths">The column widths for the match tables.</param>
    /// <param name="pageNo"> The page number for the current layout.</param>
    private void PrintPage(List<PagenationInfo> layout, Draw s, Tournament tournament, List<Score> scores, PdfDocument pdfdoc, Document doc,
    List<float> widths, int pageNo)
    {

        //Get all PagenationInfo info where pageXIndex is 0 and pageYIndex is 0
        if (layout.Count == 0) return;

        for (int i = 0; i < layout.Count; i++)
        {
            try
            {
                PagenationInfo pi = layout[i];
                
                // Handle header elements differently from match elements
                if (pi.ElementType == PageElementType.RoundHeader)
                {
                    // Create a simple paragraph for the header
                    float headerFontSize = 14f; // Fixed font size for headers
                    Paragraph headerParagraph = new Paragraph(pi.Text)
                        .SetFontSize(headerFontSize)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    
                    headerParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(headerParagraph);
                }
                else // Handle match elements
                {
                    //Find how many rows (matches) are in the round

                    
                    Match? match = s.Rounds.FirstOrDefault(e => e.Index == pi.Round)?.GetAtIndex(pi.RowOffset)?.Matches.FirstOrDefault();
                    //Find a list of scores because of multiple sets.
                    //Match is unique accross the tournament.

                    Table matchTable = new Table(widths.ToArray());
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(pi.Rectangle.Width);
                    matchTable.SetHeight(pi.Rectangle.Height);
                    matchTable.SetFixedPosition(pageNo, pi.Rectangle.Left, pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, pi.Rectangle.Width);

                    // Calculate font size in points from pixels, and scale down for better fit
                    float fontSizePx = pi.Rectangle.Height / 4.2f; // reduce divisor for smaller font
                    float fontSizePt = fontSizePx * 72f / 96f;
                    //Add a cell for the home team's CSV player
                    var homeText = match?.Home?.FirstOrDefault()?.Team?.GetPlayerCSV();
                    Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));

                    matchTable.AddCell(homeTeamCell);
                    for (int j = 0; j < tournament.Details.Sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => x.Match == match?.Id && x.Set == j + 1)?.Home.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }

                    // Add a cell for the away team's CSV player
                    var awayText = match?.Away?.FirstOrDefault()?.Team?.GetPlayerCSV();
                    Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
                    matchTable.AddCell(awayTeamCell);
                    for (int j = 0; j < tournament.Details.Sets; j++)
                    {
                        string scoreText = scores?.FirstOrDefault(x => x.Match == match?.Id && x.Set == j + 1)?.Away.ToString() ?? "";
                        Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                        matchTable.AddCell(scoreCell);
                    }
                    doc.Add(matchTable);
                }
            }
            catch (Exception ex)
            {
                // Log the error and continue with the next match
                Console.WriteLine($"Error printing match {i + 1} on page {pageNo}: {ex.Message}");
            }

        }

    }

    /// <summary>
    ///A cell with an empty bordered paragraph 
    ///and padding to look like a rectangle
    ///score box.
    /// </summary>
    /// <param name="borderSize">How thick the score box is</param>
    /// <param name="padding">Size of margin around the score box container</param>
    /// <param name="text">Optionally, text to display</param>
    /// <returns></returns>
    private Cell MakeScoreCell(float padding, string? text = "", float fontSZizePt = 16f)
    {
        var scell = new Cell();
        // Measure text size to set width/height

        // Always use 1 character width/height for the cell, regardless of text length
        float charWidth = fontSZizePt * 0.6f; // rough estimate: 0.6em per char
        float cellWidth = charWidth + 2 * padding;
        float cellHeight = fontSZizePt * 1.6f + 2 * padding; // 1.6em for height

        // Use ScoreBoxCellRenderer instead of SizableCellRenderer
        scell.SetNextRenderer(new FixedSizeCellRenderer(scell, text, cellWidth, cellHeight, 1f));
        scell.SetFontSize(fontSZizePt);
        return scell;
    }

}
