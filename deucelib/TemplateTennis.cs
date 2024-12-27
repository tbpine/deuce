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
    /// <param name="roundNo">The round to print</param>
    public void Generate(Document doc, PdfDocument pdfdoc, Schedule s, Tournament tournament, int roundNo)
    {
        // Layout : A grid for each match. Player names in the first column,
        // scores after.
        //Strategy : Use a grid for layout (easier).

        MakeHeader(doc, tournament, roundNo);

        List<Round> rounds = s.GetRounds(roundNo) ?? new();
        PdfCanvas canvas = new PdfCanvas(pdfdoc.AddNewPage());

        //Add header

        List<Table> tables = new();

        PdfFont cellFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);


        foreach (Round round in rounds)
        {
            // Calculate how many boxes to print for scores:
            int noScores = tournament?.Format?.NoSets ?? 1;

            var thome = round.GetTeamAtIndex(0);
            var taway = round.GetTeamAtIndex(1);

            //Print Round info.
            Paragraph pvs = new Paragraph($"{thome.Label} vs {taway.Label}");
            //Team membership
            Paragraph pteam1 = new Paragraph($"{thome.Label}: {thome.GetPlayerCSV()}");
            Paragraph pteam2 = new Paragraph($"{taway.Label}: {taway.GetPlayerCSV()}");
            doc.SetFontSize(10);
            doc.Add(pvs).SetFontSize(8).SetBottomMargin(2);
            pteam1.SetMarginBottom(2);
            doc.Add(pteam1);
            pteam2.SetMarginBottom(10);
            doc.Add(pteam2);

            //Dynmaically make the list of column sizes.
            //Make the sum of all column widths for scores 
            //50% of the page. Implies, that if each column width is
            //1,then the first column is the total width 
            //of all score columns.
            List<float> colWidths = [noScores];
            for (int i = 0; i < noScores; i++) colWidths.Add(1);

            foreach (Match match in round.Matches)
            {

                Table tbl = new(colWidths.ToArray());
                tbl.SetFixedLayout();
                //Set the table to 100% of page width ?
                tbl.SetWidth(UnitValue.CreatePercentValue(100));
                tbl.SetFont(cellFont);

                // Could be doubles (4 players in a match specifies a doubles match)
                //4 players, indicates doubles
                string homeName = match.IsDouble ? $"{match.GetPlayerAt(0)} / {match.GetPlayerAt(1)}"
                : $"{match.GetPlayerAt(0)}";

                string awayName = match.IsDouble ? $"{match.GetPlayerAt(2)} / {match.GetPlayerAt(3)}"
                : $"{match.GetPlayerAt(1)}";

                //Add headers
                string matchType = match.IsDouble ? "Doubles" : "Singles";
                string header1 = $"{matchType} : {homeName} vs {awayName}";
                Cell headerCell1 = new Cell().Add(new Paragraph(header1)).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5);
                tbl.AddHeaderCell(headerCell1);

                for (int i = 0; i < noScores; i++)
                {
                    Cell headerScore = new Cell().Add(new Paragraph($"Set {i + 1}")).SetBackgroundColor(ColorConstants.LIGHT_GRAY).SetPadding(5);
                    tbl.AddHeaderCell(headerScore);
                }

                //The table from iText has no rows. Cells a stack horizontally
                //with breaks at the number of col widths ( the array when the table was constructed).
                var cellHome = new Cell().Add(new Paragraph(homeName));
                cellHome.SetPadding(10);
                tbl.AddCell(cellHome);

                for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));

                var cellAway = new Cell().Add(new Paragraph(awayName));
                cellAway.SetPadding(10);
                tbl.AddCell(cellAway);

                for (int i = 0; i < noScores; i++) tbl.AddCell(MakeScoreCell(2f));
                tbl.SetMarginBottom(15f);

                tables.Add(tbl);
                doc.Add(tbl);

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
    /// <returns></returns>
    private Cell MakeScoreCell(float padding)
    {
        var scell = new Cell();
        scell.SetNextRenderer(new ScoreBoxCellRenderer(scell));
        scell.SetPadding(padding);

        return scell;
    }

    private void MakeHeader(Document doc, Tournament t, int roundNo)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        string tname = $"{t.Label}";
        //Find the date this round was held
        DateTime roundDate = t.Start.AddDays(roundNo * Utils.GetNoDays(t?.Interval ?? new Interval(3, "Daily")));

        string dates = $"{roundDate.ToString("dd MMM yyyy")}. Round {roundNo + 1}";
        string loc = $"Location :";
        string format = $"Format : {t!.Format?.NoSingles ?? 1} singles , {t.Format?.NoDoubles ?? 1} doubles, sets : {t.Format?.NoSets ?? 1}";


        doc.SetFont(font);
        doc.SetFontSize(16);
        var pname = new Paragraph(tname);
        doc.Add(pname).SetFontSize(12).SetBottomMargin(2);
        doc.Add(new Paragraph(loc)).SetTopMargin(2);
        doc.Add(new Paragraph(dates)).SetBottomMargin(2);
        doc.Add(new Paragraph(format)).SetFontSize(10).SetBottomMargin(15);


    }
}