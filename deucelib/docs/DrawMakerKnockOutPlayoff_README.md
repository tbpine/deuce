# DrawMakerKnockOutPlayoff Class

## Overview
The `DrawMakerKnockOutPlayoff` class extends the knockout tournament functionality by creating a playoff field that duplicates each round of the main tournament. This provides an additional bracket structure where each round is replicated.

## Key Features

### 1. Dual Tournament Structure
- **Main Tournament**: Standard knockout format with winner advancement
- **Playoff Field**: Duplicated rounds that mirror the main tournament structure

### 2. Tournament Creation
- Inherits the same team preparation logic as `DrawMakerKnockOut`:
  - Adds BYE teams if needed to make the total a power of 2
  - Sorts teams by ranking in descending order
  - Pairs highest vs lowest ranked teams in the first round

### 3. Round Duplication
- For each round in the main tournament, creates an identical playoff round
- Playoff matches use placeholder teams with offset IDs (1000+) to avoid conflicts
- Maintains the same bracket structure and progression logic

### 4. Factory Integration
- Added to `FactoryDrawMaker` as tournament type ID 4
- Can be instantiated using: `new DrawMakerKnockOutPlayoff(tournament, gameMaker)`

## Usage

```csharp
// Create a tournament
Tournament tournament = new Tournament() { Type = 4, /* other properties */ };

// Get game maker
IGameMaker gameMaker = new GameMakerTennis();

// Create the draw maker
var drawMaker = new DrawMakerKnockOutPlayoff(tournament, gameMaker);

// Create the draw with teams
Draw draw = drawMaker.Create(teams);
```

## Factory Usage

```csharp
FactoryDrawMaker factory = new FactoryDrawMaker();

// Using tournament type
var drawMaker = factory.Create(tournament, gameMaker); // When tournament.Type = 4

// Or using TournamentType
var tournamentType = new TournamentType(4, "Knockout Playoff", "", "", "");
var drawMaker = factory.Create(tournamentType, tournament, gameMaker);
```

## Class Structure

### Inheritance
- Inherits from `DrawMakerBase`
- Implements `IDrawMaker` interface

### Key Methods
- `Create(List<Team> teams)`: Creates the complete tournament structure with both main and playoff rounds
- `OnChange(...)`: Handles winner advancement in both main and playoff brackets
- `CreateMainTournamentRound(...)`: Creates standard knockout rounds
- `CreatePlayoffRound(...)`: Creates duplicated playoff rounds
- `CreatePlayoffBracket()`: Sets up the playoff bracket as a separate tournament

### Round Labels
- Main tournament uses standard labels (Final, Semi Final, Quarter Final)
- Playoff rounds are prefixed with "Playoff" (e.g., "Playoff Final", "Playoff Semi Final")

## Playoff Bracket Structure
The playoff bracket is created as a separate `Tournament` object linked to the main tournament through the `Bracket` system. This allows for independent management of playoff matches while maintaining the connection to the main tournament structure.

## Tournament Type IDs
- 1: Round Robin (`DrawMakerRR`)
- 2: Knockout (`DrawMakerKnockOut`)
- 3: Double Elimination (`DrawMakerBrackets`)
- 4: Knockout with Playoff (`DrawMakerKnockOutPlayoff`) - **NEW**
