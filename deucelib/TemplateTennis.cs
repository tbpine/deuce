using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace deuce;

/// <summary>
/// A class containing methods to layout elements in a PDF document
/// for tennis matches.
/// </summary>
public class TemplateTennis
{
    /// <summary>
    /// Empty constructor
    /// </summary>
    public TemplateTennis()
    {

    }

    /// <summary>
    /// Prints details of matches in a round and provides space to record scores.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="s">Collection of matches by rounds</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="round">The round to print</param>
    public void Generate(Document doc, PdfDocument pdfdoc, Schedule s, Tournament tournament, int round)
    {
        // Player names on the left (half page)
        // Could be doubles (4 players in a match specifies a doubles match)
        //Strategy : Use a grid for layout (easier).

        List<Match> matches = s.GetMatches(round) ?? new();
        PdfCanvas canvas = new PdfCanvas(pdfdoc.AddNewPage());

        //Add header
        int row = 0;
        List<Table> tables = new();

        foreach (Match match in matches)
        {
            // Calculate how many boxes to print for scores:

            int noScores = tournament?.Format?.NoSets ?? 1;
            //Dynmaically make the list of column sizes.
            //Make the sum of all column widths for scores 
            //50% of the page. Implies, that if each column width is
            //1,then the first column is the total width 
            //of all score columns.
            List<float> colWidths = new();
            colWidths.Add(noScores);
            for (int i = 0; i < noScores; i++) colWidths.Add(1);

            Table tbl = new(colWidths.ToArray());
            //Set the table to 100% of page width ?
            tbl.SetWidth(UnitValue.CreatePercentValue(100));
            //4 players, indicates doubles
            string homeName = match.Players.Count() > 2 ? $"{match.GetPlayerAt(0)} / {match.GetPlayerAt(1)}"
            : $"{match.GetPlayerAt(0)}";

            string awayName = match.Players.Count() > 2 ? $"{match.GetPlayerAt(2)} / {match.GetPlayerAt(3)}"
            : $"{match.GetPlayerAt(1)}";

            //Add headers
            tbl.AddHeaderCell(new Cell().Add(new Paragraph("Players")));

            for (int i = 0; i < noScores; i++) tbl.AddHeaderCell(new Cell().Add(new Paragraph($"Set {i + 1}")));

            //The table from iText has no rows. Cells a stack horizontally
            //with breaks at the number of col widths ( the array when the table was constructed).

            tbl.AddCell(new Cell().Add(new Paragraph(homeName)));

            for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));

            tbl.AddCell(new Cell().Add(new Paragraph(awayName)));

            for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));
            tbl.SetMarginBottom(10f);

            tables.Add(tbl);
            doc.Add(tbl);
            row++;
        }

        CreateScoreBoxes(canvas, doc, 2f, 2f, matches);


    }

    private void CreateScoreBoxes(PdfCanvas canvas, Document doc, float borderSize, float padding, List<Table> tables,
    Tournament tournament)
    {
        PdfDocument pdfdoc = canvas.GetDocument();
        Rectangle pageSize = pdfdoc.GetDefaultPageSize();
        
        
        Paragraph p = new Paragraph("");
        
        
        for (int i = 0; i < tables.Count; i++)
        {
            for(int j = 0; j < (tournament?.Format?.NoSets??0); j++)
            {
                var cell = tables[i].GetCell(1, j+1);
                

            }
            float x = (pageSize.GetWidth() - doc.GetLeftMargin() - doc.GetRightMargin()) / 2f;

            float y = pageSize.GetHeight() - doc.GetTopMargin() - 20f * row;

            Rectangle rect = new Rectangle(x, y, 10, 15);

            canvas.SetStrokeColor(ColorConstants.BLACK);
            canvas.SetLineWidth(2);
            canvas.Rectangle(rect); canvas.Stroke();

        }


    }


    /// <summary>
    ///A cell with an empty bordered paragraph 
    ///and padding to look like a rectangle
    ///score box.
    /// </summary>
    /// <param name="borderSize">How thick the score box is</param>
    /// <param name="padding">Size of margin around the score box container</param>
    /// <returns></returns>
    private Cell MakeScoreCell(float padding)
    {
        var scell = new Cell();
        scell.SetPadding(padding);

        return scell;
    }
}