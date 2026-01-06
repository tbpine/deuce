# Swiss System Tournament Format

The **Swiss system** (or Swiss format) is a type of tournament structure commonly used in competitive games like chess, Magic: The Gathering (MTG), go, bridge, and esports (e.g., Hearthstone or League of Legends qualifiers). It's designed for **large numbers of players** (often dozens to thousands) where a full round-robin (everyone plays everyone) would take too long, and single-elimination (knockout) brackets would eliminate too many strong players early.

## Key Features
- **No early eliminations**: All players participate in a **fixed number of rounds** (e.g., 5–15 rounds, depending on field size).
- **Score-based pairing**: In each round, players are paired against opponents with **similar current scores**, maximizing competitive matches.
- **Scoring**: Typically, 1 point for a win, 0.5 for a draw/tie, 0 for a loss. Final standings are based on total points, with tiebreakers (e.g., opponents' scores, head-to-head).
- **Pairing principles**:
  - Players are grouped by score (e.g., all 2-pointers together).
  - Within groups, pair strongest vs. strongest (or sometimes top vs. bottom for balance).
  - Avoid rematches if possible.
  - "Byes" for odd numbers (a player gets a free win/point).
- **Number of rounds**: Often calculated as ~log₂(n) + 1, where n is players, to ensure a clear winner (e.g., 8 players → 4 rounds; 128 players → 7–9 rounds).
- **Advantages**:
  - Fair: Top players face other top players.
  - Efficient: Determines a winner without byes or uneven games.
  - Inclusive: Weaker players get meaningful games longer.

It's named after the **Swiss Chess Federation**, which popularized it in the 1890s.

## How Pairing Works Step-by-Step
1. **Round 1**: Random pairing (or seeded by rating).
2. **Subsequent rounds**:
   - Sort all players by current score (descending).
   - Divide into score groups (e.g., 3 pts, 2.5 pts, etc.).
   - Pair within groups: If even number, pair 1st vs. 2nd, 3rd vs. 4th, etc. (or Dutch pairing: top vs. bottom).
   - Score groups with 1 player should be sorted by score highest to lowest. The player that hasn't had a bye should be given a bye.
   - Use software (e.g., Swiss-Manager, Lichess, or MTG's Wizards Event Link) for automation.
3. **End**: Highest score wins; ties broken by tiebreakers like:
   - Buchholz (sum of opponents' scores).
   - Sonneborn-Berger (weighted opponents' scores).
   - Direct encounter.

## Example 1: Small Chess Tournament (8 Players, 4 Rounds)
Let's simulate an 8-player Swiss chess tournament. Assume no draws for simplicity, random Round 1, and "top vs. bottom" pairing within score groups to keep it balanced. Players seeded A (strongest) to H (weakest).

### Round 1 (Random Pairing)
| Pairing | Result | Score After Round 1          |
|---------|--------|------------------------------|
| A vs. E | A wins | A:1, E:0                     |
| B vs. F | B wins | B:1, F:0                     |
| C vs. G | C wins | C:1, G:0                     |
| D vs. H | D wins | D:1, H:0                     |

**Scores**: 1pt: A,B,C,D | 0pt: E,F,G,H

### Round 2 (Pair within scores: Top vs. bottom)
| Pairing | Result | Score After Round 2          |
|---------|--------|------------------------------|
| A vs. D | A wins | A:2, D:1                     |
| B vs. C | B wins | B:2, C:1                     |
| E vs. H | E wins | E:1, H:0                     |
| F vs. G | F wins | F:1, G:0                     |

**Scores**: 2pt: A,B | 1pt: C,D,E,F | 0pt: G,H

### Round 3 (Pair within scores)
| Pairing | Result | Score After Round 3          |
|---------|--------|------------------------------|
| A vs. B | A wins | A:3, B:2                     |
| C vs. F | C wins | C:2, F:1                     |
| D vs. E | D wins | D:2, E:1                     |
| G vs. H | G wins | G:1, H:0                     |

**Scores**: 3pt: A | 2pt: B,C,D | 1pt: E,F,G | 0pt: H

### Round 4 (Pair within scores; A gets bye since alone at top)
| Pairing                  | Result | Final Score |
|--------------------------|--------|-------------|
| A (bye)                  | +1pt  | A:4         |
| B vs. D                  | B wins| B:3, D:2    |
| C vs. E (adjusted group) | C wins| C:3, E:1    |
| F vs. G                  | F wins| F:2, G:1    |
| H                        | -     | H:0         |

### Final Standings
| Rank | Player | Points | Tiebreaker Example (Opponents' Avg Score) |
|------|--------|--------|-------------------------------------------|
| 1    | A      | 4      | 2.25                                      |
| 2    | B      | 3      | 2.0                                       |
| 3    | C      | 3      | 1.75                                      |
| 4    | D      | 2      | 1.5                                       |
| 5    | F      | 2      | 1.0                                       |
| 6    | E      | 1      | 1.25                                      |
| 7    | G      | 1      | 0.5                                       |
| 8    | H      | 0      | 0.75                                      |

A wins undefeated. Notice how A always faced strong opponents (escalating challenge).

## Example 2: Real-World Usage
- **Chess**: FIDE World Chess Championship Candidates (14 players, 14 rounds). In 2024 Candidates, top players like Nepomniachtchi (9/14 pts) faced similar-strength opponents throughout.
- **MTG**: Pro Tours (e.g., 400+ players, 9–15 Swiss rounds + top 8 playoffs). Pairings via "Deck check" software ensure no repeats.
- **Online**: Chess.com Titled Tuesday (200+ GMs, 11 rounds). Top seed often ends ~10/11.
- **Esports**: Valorant Challengers (Swiss stage before playoffs: win 3/5 to advance, lose 3/5 eliminated—hybrid Swiss).

## Variations
- **Team Swiss**: Teams paired by score.
- **Hybrid**: Swiss rounds + single-elim playoffs (e.g., top 8).
- **Accelerated**: Top half skips early rounds.
- **Draw-heavy**: More tiebreakers needed.

Swiss keeps tournaments **exciting and fair** for large fields—strong players bubble up naturally!

---

**In the standard Swiss system** (as used in chess and defined by FIDE, which is also the basis for most Swiss tournaments worldwide, including in Switzerland), you **assign the bye first** — **before** you do the burrowing (also called "downfloating" or "upfloating" / "Floater"-Bewegungen).

Here's the typical order of operations in FIDE-style Swiss pairing (Dutch system, the most common one):

1. Sort players into score groups (highest to lowest score).
2. Pair within each score group as much as possible (usually top half vs bottom half).
3. **Handle the odd player(s) left over** — this is where the **bye** gets assigned (usually to the lowest ranked eligible player in the lowest possible score group who hasn't had a bye yet).
4. **Then** move remaining unpaired players (floaters/burrowers) to adjacent score groups as needed to complete pairings (downfloaters from higher groups, upfloaters from lower ones).

Most pairing software (Swiss-Manager, Vega, Tournament Pairing Program, etc.) follows this logic: the bye is resolved as part of the initial pairing of the score brackets, **before** the final burrowing adjustments.

So in short: **Bye first → then burrowing** is the normal sequence in the Swiss format.

(If you're using a very exotic non-FIDE variant, the order could theoretically differ — but for anything called "Swiss format" in practice, especially in chess or Swiss-organized events, the above is standard.)