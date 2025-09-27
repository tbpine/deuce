using iText.Kernel.Pdf;
using iText.Layout;

namespace deuce;

/// <summary>
/// Factory class for creating PDF templates based on tournament type.
///  This class implements the IPDFTemplateFactory interface and provides
/// /// methods to create different PDF templates for various tournament types.
/// The factory method CreateTemplate takes an integer representing the tournament type
/// /// /// </summary>
public class PDFTemplateFactory : IPDFTemplateFactory
{
    public IPDFTemplate CreateTemplate(int sport, int tournamentType)
    {
        //Switch on the sport
        switch (sport)
        {
            case 1:
                {
                    return tournamentType switch
                    {
                        1 => new PDFTemplateTennisRR(),
                        2 => new PDFTemplateTennisKO(),
                        3 => new PDFTemplateTennisKOPlayoff(),
                        4 => new PDFTemplateGroup(),
                        _ => new PDFTemplateTennisTest()
                    };
                }
            // Add cases for other sports as needed
            default:
                throw new NotSupportedException($"Sport with ID {sport} is not supported.");
        }



    }
}