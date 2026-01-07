### Swiss Tournament Pairing Rules (Simplified Implementation)

#### 1. Initial Round (Round 1)
- Sort all players by their initial ranking (highest to lowest).
- Pair players from top to bottom (1 vs 2, 3 vs 4, 5 vs 6, etc.).
- If there is an **odd number** of players:
  - The **lowest-ranked player** receives a **bye** (full points, usually 1 point).

#### 2. Subsequent Rounds
**Scoring**
- Win = **1 point**
- Draw = **½ point** (0.5)
- Loss = **0 points**
- Bye = **1 point** (usually)

**Pairing Procedure**
1. Sort all players by **current total points** (descending order).
   - Ties within the same point total are broken using standard tie-break methods (not specified here — common ones are Buchholz, Sonneborn-Berger, etc.).

2. Group players into **point groups**  
   (all players with the same number of points belong to the same group)

3. **Handling odd number of players in the tournament**
   - If the total number of players in the round is odd:
     - Sort the point groups from **lowest points to highest points**
     - Starting from the lowest point group, find the first player who has **not yet received a bye** in the tournament
     - Give that player a **bye** for this round
     - (Alternative common practice: give bye to the lowest ranked player in the lowest affected group — but your version prefers someone without previous bye)

4. **Pairing within each point group**
   For each point group (starting from the highest):

   a) **Even number of players** in the group  
      → Simply pair them within the group  
      (usually 1–2, 3–4, 5–6 etc. after proper intra-group sorting)

   b) **Odd number of players** in the group  
      → **Upfloat** (or downfloat) one player:
      - Most common implementation: **upfloat** one player from this group to the **next higher** point group
      - Alternative (used in some systems): **downfloat** one player from the **next higher** point group into this group
      - The player who gets floated is usually the one with the **worst tie-break / lowest initial rank** in the group

      After floating:
      - The group that received the floated player now has even number → pair normally
      - The group that lost the player now has even number → pair normally

### Summary – Core Principles
- Players are primarily paired with others who have **the same (or very close) score**
- Try to keep players **within their point group**
- When impossible (odd numbers) → use **byes** (for whole tournament odd count) or **float players** between adjacent score groups
- Avoid giving multiple byes to the same player when possible

This version should be much easier to implement in code or to explain to tournament organizers.

Let me know if you'd like a more formal version (e.g. with numbered steps like FIDE handbook style), or a pseudocode/flowchart version!