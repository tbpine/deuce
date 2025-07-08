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

public class TemplateTennisKO
{
    private int _numberOfMatches = 8;
    private int _numberOfRounds = 4;
    public TemplateTennisKO()
    {

    }

    public void Generate(Document doc, PdfDocument pdfdoc, Schedule s, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {

        //Set the page to be landscape
        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());
        //Get document height
        float docHeight = pdfdoc.GetDefaultPageSize().GetHeight();
        //Work out dimensions
        //Get width of each match table
        float matchTableWidth = pdfdoc.GetDefaultPageSize().GetWidth() / _numberOfRounds;
        //Get height of each match table
        float matchTableHeight = docHeight / _numberOfMatches;
    }

    private void MakeHeader(Document doc, Tournament t, int roundNo)
    {
        PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        string tname = $"{t.Label}";
        //Find the date this round was held
        Interval interval = new Interval(t.Interval, "");
        DateTime roundDate = t.Start.AddDays(roundNo * Utils.GetNoDays(interval));

        string dates = $"{roundDate.ToString("dd MMM yyyy")}. Round {roundNo + 1}";
        string loc = $"Location :";
        string format = $"Format : {t!.Details?.NoSingles ?? 1} singles , {t.Details?.NoDoubles ?? 1} doubles, sets : {t.Details?.Sets ?? 1}";


        doc.SetFont(font);
        doc.SetFontSize(16);
        var pname = new Paragraph(tname);
        doc.Add(pname).SetFontSize(12).SetBottomMargin(2);
        doc.Add(new Paragraph(loc)).SetTopMargin(2);
        doc.Add(new Paragraph(dates)).SetBottomMargin(2);
        doc.Add(new Paragraph(format)).SetFontSize(10).SetBottomMargin(15);


    }
}
