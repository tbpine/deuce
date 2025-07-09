using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

public class TemplateFactory : ITemplateFactory
{
    public ITemplate CreateTemplate(int tournamentType)
    {
        return tournamentType switch
        {
            1 => new TemplateTennis(),
            2 => new TemplateTennisKO(),
            _ => new  TemplateTennisTest()
        };
    }
}