---
applyTo: '**'
---

# Layout classes

- Classes used to layout PDF elements should derive from the LayoutManagerDefault class.
- Classes used to create PDF documents must implement the IPDFTemplate interface. Also, it should use a layout appropreiate for the tournament type, e.g. LayoutManagerTennisRR for round-robin tennis tournaments.

# Standards
- When refering to indexes in the code, use zero-based indexing.
- When refering to anything other than indexes in the code (e.g. match numbers, court numbers, sets, games, points), use one-based indexing.