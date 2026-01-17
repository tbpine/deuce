using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

/// <summary>
/// Specialized PDF printer for Swiss format tournaments.
/// Provides separate methods to print current round, standings, and next round matches.
/// Each printing operation creates its own complete document.
/// </summary>
public class PdfPrinterSwiss
{
    //------------------------------------
    //| Internals                         |
    //------------------------------------

    private readonly Draw _draw;
    private readonly IPDFTemplateFactory _templateFactory;

    /// <summary>
    /// Constructor for Swiss format PDF printer
    /// </summary>
    /// <param name="draw">Tournament draw</param>
    /// <param name="templateFactory">Factory for creating PDF templates</param>
    public PdfPrinterSwiss(Draw draw, IPDFTemplateFactory templateFactory)
    {
        _draw = draw;
        _templateFactory = templateFactory;
    }

    /// <summary>
    /// Print the current round matches to a PDF document
    /// </summary>
    /// <param name="output">Where the PDF will be stored</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="currentRound">Current round number to print</param>
    /// <param name="scores">Optionally, a list of scores to print</param>
    public async Task PrintCurrentRound(Stream output, Tournament tournament, int currentRound, List<Score>? scores = null)
    {
        // Validate inputs
        if (tournament.Type != 5) // Swiss format
        {
            throw new ArgumentException("Tournament must be Swiss format (type 5)", nameof(tournament));
        }

        // iText setup
        var pdfwriter = new PdfWriter(output);
        var pdfdoc = new PdfDocument(pdfwriter);
        var doc = new Document(pdfdoc, iText.Kernel.Geom.PageSize.A4, true);

        try
        {
            // Create Swiss template and generate current round matches
            var template = _templateFactory.CreateTemplate(tournament.Sport, tournament.Type);
            template.Generate(doc, pdfdoc, tournament, currentRound, scores);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error generating current round PDF: {ex.Message}");
            throw;
        }
        finally
        {
            // Ensure document is properly closed
            doc.Close();
            pdfdoc.Close();
            pdfwriter.Close();
            await Task.Delay(2000); // Give time for the stream to close properly
        }
    }

    /// <summary>
    /// Print the current standings to a PDF document
    /// </summary>
    /// <param name="output">Where the PDF will be stored</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="currentRound">Round number for standings calculation</param>
    public async Task PrintStandings(Stream output, Tournament tournament, int currentRound)
    {
        // Validate inputs
        if (tournament.Type != 5) // Swiss format
        {
            throw new ArgumentException("Tournament must be Swiss format (type 5)", nameof(tournament));
        }

        // iText setup
        var pdfwriter = new PdfWriter(output);
        var pdfdoc = new PdfDocument(pdfwriter);
        var doc = new Document(pdfdoc, iText.Kernel.Geom.PageSize.A4, true);

        try
        {
            // Create Swiss template and generate standings
            var swissTemplate = _templateFactory.CreateTemplate(tournament.Sport, tournament.Type);
            
            // Cast to Swiss template to access specific methods
            if (swissTemplate is PDFTemplateTennisSwiss template)
            {
                template.GenerateStandings(doc, pdfdoc, tournament, currentRound);
            }
            else
            {
                throw new InvalidOperationException("Expected Swiss template but got different type");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating standings PDF: {ex.Message}");
            throw;
        }
        finally
        {
            // Ensure document is properly closed
            doc.Close();
            pdfdoc.Close();
            pdfwriter.Close();
            await Task.Delay(2000); // Give time for the stream to close properly
        }
    }

    /// <summary>
    /// Print the next round matches to a PDF document
    /// </summary>
    /// <param name="output">Where the PDF will be stored</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="nextRound">Next round number to print</param>
    /// <param name="scores">Current scores to help with pairing calculations</param>
    public async Task PrintNextRound(Stream output, Tournament tournament, int nextRound, List<Score>? scores = null)
    {
        // Validate inputs
        if (tournament.Type != 5) // Swiss format
        {
            throw new ArgumentException("Tournament must be Swiss format (type 5)", nameof(tournament));
        }

        // iText setup
        var pdfwriter = new PdfWriter(output);
        var pdfdoc = new PdfDocument(pdfwriter);
        var doc = new Document(pdfdoc, iText.Kernel.Geom.PageSize.A4, true);

        try
        {
            // Create Swiss template and generate next round matches
            var template = _templateFactory.CreateTemplate(tournament.Sport, tournament.Type);
            template.Generate(doc, pdfdoc, tournament, nextRound, scores);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error generating next round PDF: {ex.Message}");
            throw;
        }
        finally
        {
            // Ensure document is properly closed
            doc.Close();
            pdfdoc.Close();
            pdfwriter.Close();
            await Task.Delay(2000); // Give time for the stream to close properly
        }
    }
}