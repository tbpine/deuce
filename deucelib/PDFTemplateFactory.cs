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
                        5 => new PDFTemplateTennisSwiss(),
                        _ => new PDFTemplateTennisTest()
                    };
                }
            // Add cases for other sports as needed
            default:
                throw new NotSupportedException($"Sport with ID {sport} is not supported.");
        }



    }

    /// <summary>
    /// Creates a PDF template based on the tournament's sport and type.
    /// </summary>
    /// <param name="tournament">The tournament object containing sport and type information.</param>
    /// <returns>A PDF template suitable for the tournament.</returns>
     public IPDFTemplate CreateTemplate(Tournament tournament)
    {
        return CreateTemplate(tournament.Sport, tournament.Type );

    }
}