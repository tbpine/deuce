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
        Schedule s = tournament.Schedule ?? throw new ArgumentException("Schedule cannot be null for PDF generation.");

        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        List<PagenationInfo> layout = (LayoutManager.ArrangeLayout(tournament) as List<PagenationInfo>) ?? new();
        // table widths for each match
        // The first column is wider for team names, the rest are equal for scores
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details.Sets + 1); c++) widths.Add(c == 0 ? 2f : 1f);
        //Do the first page first
        //Draw page left to right, top to bottom
        for (int pagex = 0; pagex < LayoutManager.Cols; pagex++)
        {
            for (int pagey = 0; pagey < LayoutManager.Rows; pagey++)
            {
                //Add page if not the first one
                if (pagex > 0 || pagey > 0) pdfdoc.AddNewPage();
                //Print the first page
                var layouts = layout.Where(x => x.PageXIndex == pagex && x.PageYIndex == pagey).ToList();
                PrintPage(layouts, s, tournament, scores ?? new(), pdfdoc, doc, widths);
            }
        }




        // foreach (Round round in s.Rounds)
        // {
        //     var roundLayout = layout.Where(x => x.Item1 == round.Index).ToList();
        //     int matchIndex = 0;

        //     foreach (Permutation permutation in round.Permutations)
        //     {
        //         foreach (Match match in permutation.Matches)
        //         {
        //             RectangleF recMatch = roundLayout[matchIndex].Item2;
        //             Table matchTable = new Table(widths.ToArray());
        //             matchTable.SetFixedLayout();
        //             matchTable.SetWidth(recMatch.Width);
        //             matchTable.SetHeight(recMatch.Height);
        //             matchTable.SetFixedPosition(recMatch.Left, pdfdoc.GetDefaultPageSize().GetHeight() - recMatch.Top - recMatch.Height, recMatch.Width);

        //             // Calculate font size in points from pixels, and scale down for better fit
        //             float fontSizePx = recMatch.Height / 4.2f; // reduce divisor for smaller font
        //             float fontSizePt = fontSizePx * 72f / 96f;
        //             Debug.WriteLine($"Match {matchIndex} font size: {fontSizePx}px, {fontSizePt}pt");
        //             //Get a list of scores
        //             List<Score> matchScores = scores?.Where(x => x.Id == match.Id).ToList() ?? new List<Score>();
        //             //Add a cell for the home team's CSV player
        //             var homeText = match.Home?.FirstOrDefault()?.Team?.GetPlayerCSV();
        //             Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));

        //             matchTable.AddCell(homeTeamCell);
        //             for (int i = 0; i < tournament.Details.Sets; i++)
        //             {
        //                 string scoreText = matchScores.Count > i ? matchScores[i].Home.ToString() : "";
        //                 //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
        //                 Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
        //                 matchTable.AddCell(scoreCell);
        //             }

        //             // Add a cell for the away team's CSV player
        //             var awayText = match.Away?.FirstOrDefault()?.Team?.GetPlayerCSV();
        //             Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
        //             matchTable.AddCell(awayTeamCell);
        //             for (int i = 0; i < tournament.Details.Sets; i++)
        //             {
        //                 string scoreText = matchScores.Count > i ? matchScores[i].Away.ToString() : "";
        //                 //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
        //                 Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
        //                 matchTable.AddCell(scoreCell);
        //             }
        //             doc.Add(matchTable);
        //             matchIndex++;
        //         }
        //     }
        // }
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

    private void PrintPage(List<PagenationInfo> layout, Schedule s, Tournament tournament, List<Score> scores, PdfDocument pdfdoc, Document doc,
    List<float> widths)
    {

        //Get all PagenationInfo info where pageXIndex is 0 and pageYIndex is 0
        if (layout.Count == 0) return;

        for (int i = 0; i < layout.Count; i++)
        {
            PagenationInfo pi = layout[i];
            int matchIndex = pi.PageXIndex * LayoutManager.Rows + pi.RowOffset;
            Match? match = s.Rounds.FirstOrDefault(e=>e.Index == pi.Round)?.Permutations.ElementAt(matchIndex).Matches.FirstOrDefault();
    
            var matchScores = scores?.Where(x => x.Id == match?.Id).ToList() ?? new List<Score>();

            Table matchTable = new Table(widths.ToArray());
            matchTable.SetFixedLayout();
            matchTable.SetWidth(pi.Rectangle.Width);
            matchTable.SetHeight(pi.Rectangle.Height);
            matchTable.SetFixedPosition(pi.Rectangle.Left, pdfdoc.GetDefaultPageSize().GetHeight() - pi.Rectangle.Top - pi.Rectangle.Height, pi.Rectangle.Width);

            // Calculate font size in points from pixels, and scale down for better fit
            float fontSizePx = pi.Rectangle.Height / 4.2f; // reduce divisor for smaller font
            float fontSizePt = fontSizePx * 72f / 96f;
            //Add a cell for the home team's CSV player
            var homeText = match?.Home?.FirstOrDefault()?.Team?.GetPlayerCSV();
            Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));

            matchTable.AddCell(homeTeamCell);
            for (int j = 0; j < tournament.Details.Sets; j++)
            {
                string scoreText = matchScores.Count > j ? matchScores[j].Home.ToString() : "";
                //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
                Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                matchTable.AddCell(scoreCell);
            }

            // Add a cell for the away team's CSV player
            var awayText = match?.Away?.FirstOrDefault()?.Team?.GetPlayerCSV();
            Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
            matchTable.AddCell(awayTeamCell);
            for (int j = 0; j < tournament.Details.Sets; j++)
            {
                string scoreText = matchScores.Count > j ? matchScores[j].Away.ToString() : "";
                //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
                Cell scoreCell = MakeScoreCell(2f, scoreText, fontSizePt);
                matchTable.AddCell(scoreCell);
            }
            doc.Add(matchTable);

        }

    }
}
