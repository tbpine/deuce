using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
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
        // Layout : A grid for each match. Player names in the first column,
        // scores after.
        //Strategy : Use a grid for layout (easier).
        MakeHeader(doc, tournament);

        List<Match> matches = s.GetMatches(round) ?? new();
        PdfCanvas canvas = new PdfCanvas(pdfdoc.AddNewPage());

        //Add header
        
        List<Table> tables = new();

        PdfFont cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

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

            // Could be doubles (4 players in a match specifies a doubles match)
            //4 players, indicates doubles
            string homeName = match.Players.Count() > 2 ? $"{match.GetPlayerAt(0)} / {match.GetPlayerAt(1)}"
            : $"{match.GetPlayerAt(0)}";

            string awayName = match.Players.Count() > 2 ? $"{match.GetPlayerAt(2)} / {match.GetPlayerAt(3)}"
            : $"{match.GetPlayerAt(1)}";

            //Add headers
            string header1 = $"Round {round} : {homeName} vs {awayName}";
            Cell headerCell1 = new Cell().Add(new Paragraph(header1)).SetFont(cellFont).SetFontSize(12).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5);
            tbl.AddHeaderCell(headerCell1);

            for (int i = 0; i < noScores; i++) 
            {
                Cell headerScore = new Cell().Add(new Paragraph($"Set {i + 1}")).SetFont(cellFont).SetFontSize(12).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5);
                tbl.AddHeaderCell(headerScore);
            }

            //The table from iText has no rows. Cells a stack horizontally
            //with breaks at the number of col widths ( the array when the table was constructed).

            tbl.AddCell(new Cell().Add(new Paragraph(homeName))).SetFont(cellFont).SetFontSize(12).SetPadding(5);

            for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));

            tbl.AddCell(new Cell().Add(new Paragraph(awayName))).SetFont(cellFont).SetFontSize(12).SetPadding(5);

            for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));
            tbl.SetMarginBottom(15f);

            tables.Add(tbl);
            doc.Add(tbl);
         
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
        scell.SetNextRenderer(new ScoreBoxCellRenderer(scell));
        scell.SetPadding(padding);

        return scell;
    }

    private void MakeHeader(Document doc, Tournament t)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        string tname = $"{t.Label}";
        string dates = $"From : {t.Start.ToString("dd MMM yyyy")} To: {t.Start.ToString("dd MMM yyyy")}";
        string duration = $"When: {t?.Interval?.Label??""}";

        doc.Add(new Paragraph(tname)).SetFont(font).SetFontSize(18).SetTopMargin(10);
        doc.Add(new Paragraph(dates)).SetFont(font).SetFontSize(16);
        doc.Add(new Paragraph(duration)).SetFont(font).SetFontSize(16).SetBottomMargin(25);
        
        
    }
}