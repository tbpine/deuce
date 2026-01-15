# Swiss Tournament Standings PDF Generation

This functionality allows you to generate PDF documents showing the current standings for a specific round in a Swiss System tournament. You can generate standalone standings or combined standings with match details.

## Features

- **Round-specific standings**: Generate standings for any completed round
- **Combined reports**: Generate standings with round matches in a single PDF
- **Comprehensive table**: Shows position, team name, wins, losses, draws, and points
- **Tournament summary**: Displays key statistics like average points, leading teams, and ties
- **Professional formatting**: Clean, readable layout with alternating row colors
- **Player information**: Includes player names for each team when available
- **Match details**: Shows match pairings and score boxes when combined with matches

## Usage

### Combined Standings and Matches (Recommended)

```csharp
// Generate PDF with both standings and matches for a specific round
Tournament tournament = GetSwissTournament(); // Your method to get tournament data
int roundNumber = 2; // The round you want to show (1-based)
string outputPath = "Swiss_Complete_Round_2.pdf";
List<Score>? scores = GetScoresForRound(roundNumber); // Optional scores

// Generate the combined PDF
bool success = SwissStandingsPdfExample.GenerateStandingsWithMatchesPdf(tournament, roundNumber, outputPath, scores);
```

### Standalone Standings Only

```csharp
// Generate PDF with only standings for a specific round
Tournament tournament = GetSwissTournament(); 
int roundNumber = 2; 
string outputPath = "Swiss_Standings_Round_2.pdf";

// Generate standings-only PDF
bool success = SwissStandingsPdfExample.GenerateStandingsPdf(tournament, roundNumber, outputPath);
```

### Using the PDFTemplate Directly

```csharp
using iText.Kernel.Pdf;
using iText.Layout;

// Create PDF writer and document
using var writer = new PdfWriter("combined_report.pdf");
using var pdfDoc = new PdfDocument(writer);
using var document = new Document(pdfDoc);

// Create the Swiss PDF template
var pdfTemplate = new PDFTemplateTennisSwiss();

// Generate combined standings and matches for round 3
pdfTemplate.GenerateStandingsWithMatches(document, pdfDoc, tournament, 3, scores);

// OR generate standings only
pdfTemplate.GenerateStandings(document, pdfDoc, tournament, 3);
```

## Requirements

- Tournament must have standings data for the requested round (for standings display)
- Tournament should have a proper draw structure with rounds and matches (for combined reports)
- Tournament should have proper team and player information
- The round number should be 1-based (Round 1, Round 2, etc.)

## Combined PDF Content

The combined report (`GenerateStandingsWithMatches`) includes:

### For Round 1:
1. **Matches Section** (current round)
   - Match pairings for the round
   - Score recording boxes
   - Player information for each team
2. **Page Break**  
3. **Standings Section** (after the round)
   - Current standings table
   - Tournament summary statistics

### For Round 2+:
1. **Standings Section** (previous round standings)
   - Shows standings after the previous round
2. **Page Break**
3. **Matches Section** (current round)
   - Match pairings for the current round
   - Score recording boxes

## Standalone Standings PDF Content

The standalone report includes:

1. **Header Section**:
   - Tournament name and format type
   - Date and round information  
   - Tournament format details (singles, doubles, sets)

2. **Standings Table**:
   - Position ranking
   - Team names with player information
   - Match statistics (wins, losses, draws)
   - Current points total

3. **Summary Section**:
   - Total number of teams
   - Average points across all teams
   - Current leading score
   - Teams tied for the lead (if applicable)

## Error Handling

- If no standings exist for the requested round, a message is displayed in the PDF
- Invalid input parameters throw appropriate exceptions
- File I/O errors are caught and reported
- Missing match data is handled gracefully

## Example Usage in Your Code

```csharp
public class TournamentManager
{
    public void GenerateRoundReports(Tournament tournament, int currentRound)
    {
        // Generate complete report with standings and matches
        string completePath = $"Tournament_{tournament.Id}_Round_{currentRound}_Complete.pdf";
        var scores = GetScoresForTournament(tournament.Id, currentRound);
        
        SwissStandingsPdfExample.GenerateStandingsWithMatchesPdf(
            tournament, currentRound, completePath, scores);

        // Also generate standalone standings for quick reference
        string standingsPath = $"Tournament_{tournament.Id}_Round_{currentRound}_Standings.pdf";
        SwissStandingsPdfExample.GenerateStandingsPdf(
            tournament, currentRound, standingsPath);
    }
}
```

## Integration with Tournament Management

This functionality integrates seamlessly with existing Swiss tournament management:

- Works with `DrawMakerSwiss` for tournament generation
- Uses `Tournament.GetStandingsForRound()` for data retrieval
- Uses existing `Draw` and `Round` structures for match information
- Compatible with existing PDF generation workflow
- Follows the same layout patterns as other tournament formats
- Supports the existing `Score` system for match results