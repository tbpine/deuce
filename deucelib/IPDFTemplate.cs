using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

/// <summary>
/// Interface for PDF template generation.
/// This interface defines the contract for generating PDF documents for tournament schedules.  
/// It includes a layout manager for arranging elements in the PDF and a method to generate the document.
/// Implementations of this interface should provide the logic for creating a PDF document based on the provided
/// </summary>
public interface IPDFTemplate
{
    ILayoutManager LayoutManager { get; }
    void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int round, List<Score>? scores);
    void GenerateStandings(Document doc, PdfDocument pdfdoc, Tournament tournament, int round);
}