using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using deuce.ext;

namespace deuce;

public class PDFTemplateTennisKOPlayoff : IPDFTemplate
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

    public PDFTemplateTennisKOPlayoff()
    {
        LayoutManager = new LayoutManagerTennisKOPlayoff(
            PageSize.A4.Rotate().GetWidth(),
            PageSize.A4.Rotate().GetHeight(),
            _page_top_margin, _page_left_margin, _page_right_margin, _page_bottom_margin,
            _table_padding_top, _table_padding_bottom, _table_padding_left, _table_padding_right
        );
    }

    public void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        //What inputs from the layout have i got ?
        //Can i find the match that i'm meant to be printing ?
        // If i can't, then should i change the layout manager.

        Draw s = tournament.Draw ?? throw new ArgumentException("Schedule cannot be null for PDF generation.");

        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        List<PagenationInfo> layout = (LayoutManager.ArrangeLayout(tournament) as List<PagenationInfo>) ?? new();

        // table widths for each match
        // The first column is wider for team names, the rest are equal for scores
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details.Sets + 1); c++) widths.Add(c == 0 ? 2f : 1f);

        //Seperate printing for  playoff rounds from the main rounds
        //Print the main round first.
        //Group by page
        var groupedLayout = from p in layout
                            where !p.IsPlayoffRound && !p.IsFinalMatch
                            group p by p.PageIndex into g
                            select new { PageIndex = g.Key, Layouts = g.ToList() };
        foreach (var group in groupedLayout)
        {
            //Add a new page for each group
            pdfdoc.AddNewPage();
            //Print the matches on this page
            //Find ind matches associated with this layout

            PrintPage(group.Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, group.PageIndex);
        }

        //Print the playoff rounds
        var groupedLayoutPlayoff = from p in layout
                                   where p.IsPlayoffRound && !p.IsFinalMatch
                                   group p by p.PageIndex into g
                                   select new { PageIndex = g.Key, Layouts = g.ToList() };
        foreach (var group in groupedLayoutPlayoff)
        {
            //Add a new page for each group
            pdfdoc.AddNewPage();
            //Print the matches on this page
            //Find ind matches associated with this layout

            PrintPage(group.Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, group.PageIndex);
        }

        //Print the final match page
       
        //Print the playoff rounds
        var groupFinalMatch = from p in layout
                                   where p.IsFinalMatch
                                   group p by p.PageIndex into g
                                   select new { PageIndex = g.Key, Layouts = g.ToList() };
        if (groupFinalMatch.FirstOrDefault() != null)
        {
            pdfdoc.AddNewPage();
            PrintPage(groupFinalMatch.First().Layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths, groupFinalMatch.First().PageIndex);
        }

    }

    private void PrintPage(List<PagenationInfo> layout, Draw draw, Tournament tournament, List<Score> scores, PdfDocument pdfdoc, Document doc,
    List<float> widths, int pageNo)
    {

        //Get all PagenationInfo info where pageXIndex is 0 and pageYIndex is 0
        if (layout.Count == 0) return;

        for (int i = 0; i < layout.Count; i++)
        {
            try
            {
                PagenationInfo pi = layout[i];
                
                // Handle header and label elements differently from match elements
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
                else if (pi.ElementType == PageElementType.RoundLabel)
                {
                    // Create a paragraph for the round label (Main/Playoff)
                    float labelFontSize = 16f; // Slightly larger font size for labels
                    Paragraph labelParagraph = new Paragraph(pi.Text)
                        .SetFontSize(labelFontSize)
                        .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
                    
                    labelParagraph.SetFixedPosition(pageNo, pi.Rectangle.Left, 
                        pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, 
                        pi.Rectangle.Width);
                    
                    doc.Add(labelParagraph);
                }
                else // Handle match elements
                {
                    //Find how many rows (matches) are in the round

                    var round = pi.IsPlayoffRound ? draw.Rounds.FirstOrDefault(e => e.Index == pi.Round)?.Playoff :
                             draw.Rounds.FirstOrDefault(e => e.Index == pi.Round);

                    Match? match = round?.Permutations.FirstOrDefault(p => p.Id == pi.RowOffset)?.Matches.FirstOrDefault(); 

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
                    var homeText = match?.Home?.FirstOrDefault()?.Team?.GetPlayerCSV() ?? "";
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
