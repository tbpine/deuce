using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

/// <summary>
/// Base class for PDF template generation.
/// This class provides default implementations for the IPDFTemplate interface
/// that can be overridden by derived classes for specific tournament formats.
/// </summary>
public abstract class PDFTemplateBase : IPDFTemplate
{
    /// <summary>
    /// Gets the layout manager for arranging elements in the PDF.
    /// </summary>
    public virtual ILayoutManager LayoutManager { get; protected set; } = null!;

    /// <summary>
    /// Generates the PDF document for a tournament showing matches for a specific round.
    /// This method should only display match information and should NOT generate standings.
    /// Use GenerateStandings() method separately when standings are needed.
    /// This is a virtual method that should be overridden by derived classes.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="round">The round to generate</param>
    /// <param name="scores">Optionally, a list of scores to display</param>
    public virtual void Generate(Document doc, PdfDocument pdfdoc, Tournament tournament, int round, List<Score>? scores)
    {
        // Default implementation - derived classes should override this method
        throw new NotImplementedException("Generate method must be implemented by derived classes.");
    }

    /// <summary>
    /// Generates a PDF showing ONLY the standings for a specific round in a tournament.
    /// This is the ONLY method that should generate standings - the Generate method should NOT include standings.
    /// This provides clear separation of concerns between match display and standings display.
    /// This is a virtual method that provides a default empty implementation.
    /// </summary>
    /// <param name="doc">iText document object</param>
    /// <param name="pdfdoc">PDF document object</param>
    /// <param name="tournament">Tournament details</param>
    /// <param name="round">The round for which to show standings</param>
    public virtual void GenerateStandings(Document doc, PdfDocument pdfdoc, Tournament tournament, int round)
    {
        // Default implementation - does nothing
        // Only certain tournament formats (like Swiss) need standings generation
    }
}