# duece 
The tournament organiser
## Round robin
Turns out that matching the upper half of the array with the lower half then
rotating the array every round is how you make games.

## TODOs
- [X] Configure players in a match
- [X] Load/Save matches and rounds (with unit testing)
    -[X] Rebuild schedule from the database
    -[X] Match players seperated into 2 lists
    -[X] Foriegn key constraints
- [X] Knockout format
- [X] Print out for knockout format
- [X] CRUD tournament
- [X] Tournament wizard
- [X] Web Page tournament
- [X] Unit tests
- [X] Score cards, schedule
- [X] Team based matches
    - [X] Just make sure you can do 2 singles and 2 doubles.
- [X] Update to .NET version 8
- [X] Schedule contains rounds instead of matches
    - [X] Use cell renderers
- [X] Refactor library
- [X] Printing
- [] Investigate Game makers using null teams
- [X] Rework draw advancement for the knock out tournament with scores as inputs.

- [] All draw makers should use the teams reference inside the tournament, not the from function parameters.