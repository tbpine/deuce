using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

/// <summary>
/// Create a PDF document from any schedule.
/// </summary>
public class PdfPrinter
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------

    private readonly Schedule _schedule;

    /// <summary>
    /// Set schedule to print
    /// </summary>
    /// <param name="s">Schedule to print</param>
    /// <param name="scores">Optionally, a list of scores</param>
    public PdfPrinter(Schedule s)
    {
        _schedule = s;
    }

    /// <summary>
    /// Print to an output stream
    /// </summary>
    /// <param name="output">Where the pdf will be stored</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="s">Schedule</param>
    /// <param name="round">Print only matches for this round</param>
    /// <param name="scores">Optionally, a list of scores to print</param>
    public async Task Print(Stream output, Tournament tournament, Schedule s, int round, List<Score>? scores = null)
    {

        //iText set up
        var pdfwriter = new PdfWriter(output);
        var pdfdoc = new PdfDocument(pdfwriter);
        var doc = new Document(pdfdoc, iText.Kernel.Geom.PageSize.A4, true);

        //Generate the document based on
        //the type of sport played.
        if (tournament.Sport == 1)
        {
            TemplateTennis template = new();

            template.Generate(doc, pdfdoc, s, tournament, round, scores);
        }
        else
        {
            //TODO: Another class that
            //prints the defult teams round robbin.

        }


        //Set PDF byte stream to the output.
        doc.Close(); 
        pdfdoc.Close(); // Close the document to ensure all content is written.
        pdfwriter.Close();
        await Task.Delay(2000); // Give time for the stream to close properly.
    }
}
