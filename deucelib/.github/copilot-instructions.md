---
applyTo: '**'
---

# Layout classes

- Classes used to layout PDF elements should derive from the LayoutManagerDefault class.
- Classes used to create PDF documents must implement the IPDFTemplate interface. Also, it should use a layout appropreiate for the tournament type, e.g. LayoutManagerTennisRR for round-robin tennis tournaments.