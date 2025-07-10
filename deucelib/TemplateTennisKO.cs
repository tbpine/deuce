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

namespace deuce;

public class TemplateTennisKO : ITemplate
{
    private int _max_rows = 8;
    private float _page_top_margin = 10f;
    private float _page_bottom_margin = 10f;
    private float _page_left_margin = 10f;
    private float _page_right_margin = 10f;

    private float _table_padding_top = 5f;
    private float _table_padding_bottom = 5f;
    private float _table_padding_left = 5f;
    private float _table_padding_right = 5f;

    public TemplateTennisKO()
    {

    }

    public void Generate(Document doc, PdfDocument pdfdoc, Schedule s, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        //Set the page to be landscape
        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());
        //Get document height minus padding
        LayoutManager lm = new LayoutManager(pdfdoc.GetDefaultPageSize().GetWidth(),
            pdfdoc.GetDefaultPageSize().GetHeight(), _page_top_margin, _page_left_margin, _page_right_margin,
            _page_bottom_margin, _table_padding_top, _table_padding_bottom, _table_padding_left, _table_padding_right);

        //Get the number of matches in the first round
        int roundOneMatches = s.Rounds.FirstOrDefault(x=>x.Index == 1)?.Permutations.Sum(x => x.Matches.Count) ?? 0;
        roundOneMatches = roundOneMatches <= _max_rows ? roundOneMatches : _max_rows;

        var layout = lm.Calculate(roundOneMatches);
        //Get table widths
        List<float> widths = new List<float>();
        for (int c = 0 ; c < (tournament.Details.Sets+1); c++) widths.Add(c== 0 ? 2f : 1f);

        foreach (Round round in s.Rounds)
        {
            //Get layout for this round
            var roundLayout = layout.Where(x => x.Item1 == round.Index).ToList();
            int matchIndex = 0;

            foreach (Permutation permutation in round.Permutations)
            {
                foreach (Match match in permutation.Matches)
                {
                    RectangleF recMatch = roundLayout[matchIndex].Item2;
                    //Add a table for the match
                    Table matchTable = new Table(widths.ToArray());
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(recMatch.Width);
                    matchTable.SetHeight(recMatch.Height);
                    //Location , pdf starts at button left corner and expands upwards
                    matchTable.SetFixedPosition(recMatch.Left, pdfdoc.GetDefaultPageSize().GetHeight() - recMatch.Top  - recMatch.Height, recMatch.Width);

                    //Get a list of scores
                    List<Score> matchScores = scores?.Where(x => x.Id == match.Id).ToList() ?? new List<Score>();
                    //Add a cell for the home team's CSV player
                    Cell homeTeamCell = new Cell().Add(new Paragraph(match.Home?.FirstOrDefault()?.Team?.GetPlayerCSV()));
                    matchTable.AddCell(homeTeamCell);
                    //For each set, add a score cell
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        Cell scoreCell = new Cell().Add(new Paragraph(matchScores.Count > i ? matchScores[i].Home.ToString() : ""));
                        matchTable.AddCell(scoreCell);
                    }

                    // Add a cell for the away team's CSV player
                    Cell awayTeamCell = new Cell().Add(new Paragraph(match.Away?.FirstOrDefault()?.Team?.GetPlayerCSV()));
                    matchTable.AddCell(awayTeamCell);
                    //For each set, add a score cell
                    for (int i = 0; i < tournament.Details.Sets; i++)
                    {
                        Cell scoreCell = new Cell().Add(new Paragraph(matchScores.Count > i ? matchScores[i].Away.ToString() : ""));
                        matchTable.AddCell(scoreCell);
                    }
                    //Add table to the document
                    doc.Add(matchTable);
                    matchIndex++;
                }
            }
        }
                
    }


}
