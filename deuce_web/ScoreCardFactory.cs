using deuce;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

/// <summary>
/// 
/// </summary>
class ScoreCardFactory
{
    private readonly ILogger<ScoreCardFactory> _log;
    public ScoreCardFactory(ILogger<ScoreCardFactory> log)
    {
        _log = log;
    }

    public async Task Generate(Stream output, Tournament t, List<Player> players)
    {
        _log.LogInformation($"Tournament {t.Label}|{t.Type}");
        FactoryCreateMatchFactory fac  = new FactoryCreateMatchFactory();
        var matchfac = fac.Create(t);
        //Dictionary
        Schedule s = matchfac.Produce(players);

        var writer = new PdfWriter(output);
        var pdf = new PdfDocument(writer);
        //Make sure we're using A4
        pdf.SetDefaultPageSize(PageSize.A4);
        
        var doc = new Document(pdf);
        
        
        for(int r = 0; r < s.NoRounds; r++)
        {
            Paragraph pround = new Paragraph($"Round #{r}");
            doc.Add(pround);
            //Add a paragraph for each match
            foreach(var match in  s.GetMatches(r)??new List<Match>())
            {
                Paragraph p1 = new($"{match.Players.First()}\n{match.Players.Last()}\n");
                doc.Add(p1);
            }
        }

        await Task.CompletedTask;
        

    }
}