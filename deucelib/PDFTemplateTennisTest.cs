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

public class PDFTemplateTennisTest : PDFTemplateBase
{
    public override ILayoutManager LayoutManager { get; protected set; }
    public PDFTemplateTennisTest()
    {
        LayoutManager = new LayoutManagerDefault(
            595f, 842f, // A4 size
            36f, 36f, 36f, 36f, // Margins
            5f, 5f, // Padding
            5f, 5f // Table padding
        );
    }

    public override void Generate(Document doc, PdfDocument pdfdoc,  Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {
        // Set the page to be landscape
        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());
        // Store page dimensions
        float docHeight = pdfdoc.GetDefaultPageSize().GetHeight();
        float docWidth = pdfdoc.GetDefaultPageSize().GetWidth();
        //Debug output
        Console.WriteLine($"Document Height: {docHeight}, Width: {docWidth}");

        int cols = 4;
        int rows = 8;

        //Caculate cell width and height
        float cellWidth = docWidth / cols;
        float cellHeight = docHeight / rows;


        //make a table with 2 colummns
        Table matchTable = new(new float[]{2f,1f});
        matchTable.SetFixedLayout();
        //Set the table to 100% of page width ?
        matchTable.SetWidth(cellWidth);
        matchTable.SetHeight(cellHeight);
                
        // Add a cell for the home team's CSV player
        Cell homeTeamCell = new Cell().Add(new Paragraph("Home Team"));
        matchTable.AddCell(homeTeamCell);
        //Add a blank cell for the home team's score
        Cell homeScoreCell = new Cell().Add(new Paragraph("Home Score"));
        matchTable.AddCell(homeScoreCell);
        // Add a cell for the away team's CSV player
        Cell awayTeamCell = new Cell().Add(new Paragraph("Away Team"));
        matchTable.AddCell(awayTeamCell);
        //Add a blank cell for the away team's score
        Cell awayScoreCell = new Cell().Add(new Paragraph("Away Score"));
        matchTable.AddCell(awayScoreCell);

        //Add table to the document
        doc.Add(matchTable);
    }

  
}
