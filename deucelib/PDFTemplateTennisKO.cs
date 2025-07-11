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

    public void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        Schedule s = tournament.Schedule ?? throw new ArgumentException("Schedule cannot be null for PDF generation.");

        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());

        List<(int, RectangleF)> layout = (LayoutManager.ArrangeLayout(tournament) as List<(int, RectangleF)>) ?? new();
        //Get table widths
        List<float> widths = new List<float>();
        for (int c = 0; c < (tournament.Details.Sets + 1); c++) widths.Add(c == 0 ? 2f : 1f);

        foreach (Round round in s.Rounds)
        {
            var roundLayout = layout.Where(x => x.Item1 == round.Index).ToList();
            int matchIndex = 0;

            foreach (Permutation permutation in round.Permutations)
            {
                foreach (Match match in permutation.Matches)
                {
                    RectangleF recMatch = roundLayout[matchIndex].Item2;
                    Table matchTable = new Table(widths.ToArray());
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(recMatch.Width);
                    matchTable.SetHeight(recMatch.Height);
                    matchTable.SetFixedPosition(recMatch.Left, pdfdoc.GetDefaultPageSize().GetHeight() - recMatch.Top - recMatch.Height, recMatch.Width);

                    // Calculate font size in points from pixels, and scale down for better fit
                    float fontSizePx = recMatch.Height / 4.2f; // reduce divisor for smaller font
                    float fontSizePt = fontSizePx * 72f / 96f;
                    Debug.WriteLine($"Match {matchIndex} font size: {fontSizePx}px, {fontSizePt}pt");
                    //Get a list of scores
                    List<Score> matchScores = scores?.Where(x => x.Id == match.Id).ToList() ?? new List<Score>();
                    //Add a cell for the home team's CSV player
                    var homeText = match.Home?.FirstOrDefault()?.Team?.GetPlayerCSV();
                    Cell homeTeamCell = new Cell().Add(new Paragraph(homeText).SetFontSize(fontSizePt));

                    matchTable.AddCell(homeTeamCell);
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        string scoreText = matchScores.Count > i ? matchScores[i].Home.ToString() : "";
                        //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
                        Cell scoreCell = MakeScoreCell(2f, scoreText,fontSizePt );
                        matchTable.AddCell(scoreCell);
                    }

                    // Add a cell for the away team's CSV player
                    var awayText = match.Away?.FirstOrDefault()?.Team?.GetPlayerCSV();
                    Cell awayTeamCell = new Cell().Add(new Paragraph(awayText).SetFontSize(fontSizePt));
                    matchTable.AddCell(awayTeamCell);
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        string scoreText = matchScores.Count > i ? matchScores[i].Away.ToString() : "";
                        //Cell scoreCell = new Cell().Add(new Paragraph(scoreText).SetFontSize(fontSizePt));
                        Cell scoreCell = MakeScoreCell(2f, scoreText,fontSizePt );
                        matchTable.AddCell(scoreCell);
                    }
                    doc.Add(matchTable);
                    matchIndex++;
                }
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
        scell.SetNextRenderer(new SizableCellRenderer(scell, [0.2f, 0.1f, 0.2f, 0.1f]));
        scell.SetFontSize(fontSZizePt);

        return scell;
    }

}
