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
using System.Diagnostics;

namespace deuce;

public class TemplateTennisKO : ITemplate
{
    private int _rows = 8;
    private int _cols = 4;
    public TemplateTennisKO()
    {

    }

    public void Generate(Document doc, PdfDocument pdfdoc, Schedule s, Tournament tournament, int roundNo,
    List<Score>? scores = null)
    {

        //Set the page to be landscape
        pdfdoc.SetDefaultPageSize(PageSize.A4.Rotate());
        //Get document height
        float docHeight = pdfdoc.GetDefaultPageSize().GetHeight();
        //Work out dimensions
        //Get width of each match table
        float tableWidth = pdfdoc.GetDefaultPageSize().GetWidth() / _cols;
        //Get height of each match table
        float tableHeight = docHeight / _rows;

        double drawAreaWidth = pdfdoc.GetDefaultPageSize().GetWidth() / _cols;
        // Create tables for each match in all rounds
        foreach (var round in s.Rounds)
        {
            double drawAreaHeight = docHeight / _rows / Math.Pow(2, round.Index); // Adjust height based on round index
            int matchInRow = 0;
            foreach (var permutation in round.Permutations)
            {
                foreach (var match in permutation.Matches)
                {
                    // Get all scores for the match
                    var matchScores = scores?.Where(s => s.Match == match.Id).ToList();

                    int numColumns = tournament.Details?.Sets  ?? 0 + 1; // Number of scores + 1, multiplied by 2
                    //Make a column width array
                    List<float> colWidths = new List<float>();
                    // Calculate the number of columns based on the number of scores
                    //first column is twice the size of the others
                    for (int i = 0; i < numColumns; i++) colWidths.Add(i == 0? 2f : 1f);

                    //Number of rows equals total number of columns divided by columns per row        
                    Table matchTable = new Table(colWidths.ToArray());
                    //Set table fixed layout
                    matchTable.SetFixedLayout();
                    matchTable.SetWidth(tableWidth);
                    matchTable.SetHeight(tableHeight);

                    // Calculate horizontal position based on round and match index
                    double xPosition = (round.Index-1) * drawAreaWidth;
                    //Adjust hortizontal alignment to the center of the drawAreaWidth
                    xPosition +=  (drawAreaWidth - tableWidth) / 2;

                    double yPosition = docHeight -  matchInRow * drawAreaHeight;
                    //Debug out the x and y positions
                    Debug.WriteLine($"Match: {match.Id}, Round: {round.Index}, X: {xPosition}, Y: {yPosition}");

                    // Adjust vertical position aligment to the center of the drawAreaHeight

                    matchTable.SetFixedPosition((float)xPosition, (float)yPosition, tableWidth);

                    // Add padding around the table
                    matchTable.SetPadding(5);

                    // Add a cell for the home team's CSV player
                    Cell homeTeamCell = new Cell().Add(new Paragraph(match.Home.FirstOrDefault()?.Team?.GetPlayerCSV()));
                    matchTable.AddCell(homeTeamCell);

                    // Add numColumns - 1 number of cells
                    for (int col = 0; col < (numColumns - 2) / 2; col++)
                    {
                        var score = col < matchScores?.Count ? matchScores[col] : null;

                        Cell scoreCell = new Cell();
                        if (score != null) scoreCell.Add(new Paragraph(score.Home.ToString()));
                        matchTable.AddCell(scoreCell);
                    }

                    // Add a cell for the home team's CSV player
                    Cell awayTeamCell = new Cell().Add(new Paragraph(match.Away.FirstOrDefault()?.Team?.GetPlayerCSV()));
                    matchTable.AddCell(awayTeamCell);

                    // Add numColumns - 1 number of cells
                    for (int col = 0; col < (numColumns - 2) / 2; col++)
                    {
                        var score = col < matchScores?.Count ? matchScores[col] : null;

                        Cell scoreCell = new Cell();
                        if (score != null) scoreCell.Add(new Paragraph(score.Away.ToString()));

                        matchTable.AddCell(scoreCell);
                    }

                    // Add the table to the document
                    doc.Add(matchTable);
                    matchInRow++;
                }
            }
        }
    }

  
}
