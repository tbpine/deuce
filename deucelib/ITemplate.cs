using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

public interface ITemplate
{
    void Generate(Document doc, PdfDocument pdfdoc, Schedule schedule, Tournament tournament, int round, List<Score>? scores);
}