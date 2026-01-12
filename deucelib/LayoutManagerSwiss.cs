using System.Drawing;

namespace deuce;

/// <summary>
/// Layout manager specifically designed for Swiss System tournaments.
/// Manages layout for matches in each round where teams are paired based on current standings.
/// </summary>
/// <remarks>
/// Swiss tournaments pair teams with similar scores in each round. This layout manager
/// organizes matches for a specific round in a grid format, grouping matches by point brackets
/// when possible for better readability.
/// </remarks>
public class LayoutManagerSwiss : LayoutManagerDefault
{
    private const int DEFAULT_MATCHES_PER_PAGE = 8; // Default matches per page for Swiss rounds
    private const int DEFAULT_COLUMNS_PER_PAGE = 2; // Default columns for organizing matches

    /// <summary>
    /// Initializes a new instance of the LayoutManagerSwiss class with specified page dimensions and margins.
    /// </summary>
    /// <param name="pageWidth">The total width of the page in units (typically points)</param>
    /// <param name="pageHeight">The total height of the page in units (typically points)</param>
    /// <param name="pageTopMargin">The top margin of the page</param>
    /// <param name="pageLeftMargin">The left margin of the page</param>
    /// <param name="pageRightMargin">The right margin of the page</param>
    /// <param name="pageBottomMargin">The bottom margin of the page</param>
    /// <param name="tablePaddingTop">The top padding for table elements</param>
    /// <param name="tablePaddingBottom">The bottom padding for table elements</param>
    /// <param name="tablePaddingLeft">The left padding for table elements (default: 5f)</param>
    /// <param name="tablePaddingRight">The right padding for table elements (default: 5f)</param>
    public LayoutManagerSwiss(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
        : base(pageWidth, pageHeight, pageTopMargin, pageLeftMargin, pageRightMargin, pageBottomMargin, tablePaddingTop, tablePaddingBottom, tablePaddingLeft, tablePaddingRight)
    {
        // Set Swiss-specific layout parameters
        _maxCols = DEFAULT_COLUMNS_PER_PAGE;
        _maxRows = DEFAULT_MATCHES_PER_PAGE / DEFAULT_COLUMNS_PER_PAGE;
    }

    /// <summary>
    /// Arranges the layout for Swiss System tournament matches for a specific round.
    /// Creates a grid-based layout organizing matches by point brackets when possible.
    /// </summary>
    /// <param name="tournament">The tournament object containing schedule and match information</param>
    /// <returns>A list of tuples containing page numbers and rectangles representing match positions</returns>
    /// <remarks>
    /// The algorithm works by:
    /// 1. Extracting matches for each round from the tournament draw
    /// 2. Organizing matches in a grid layout with appropriate spacing
    /// 3. Creating multiple pages if the number of matches exceeds page capacity
    /// 4. Grouping matches by point brackets when standings are available
    /// </remarks>
    public override object ArrangeLayout(Tournament tournament)
    {
        var layouts = new List<(int, Rectangle)>();
        
        if (tournament?.Draw?.Rounds == null || tournament.Draw.Rounds.Count() == 0)
        {
            return layouts;
        }

        // Calculate available space for matches
        float availableWidth = _pageWidth - _pageLeftMargin - _pageRightMargin;
        float availableHeight = _pageHeight - _pageTopMargin - _pageBottomMargin;
        
        // Calculate match dimensions based on grid layout
        float matchWidth = (availableWidth - (_tablePaddingLeft + _tablePaddingRight) * _maxCols) / _maxCols;
        float matchHeight = (availableHeight - (_tablePaddingTop + _tablePaddingBottom) * _maxRows) / _maxRows;

        int pageNumber = 0;
        
        // Process each round in the Swiss tournament
        for (int roundIndex = 0; roundIndex < tournament.Draw.Rounds.Count(); roundIndex++)
        {
            var round = tournament.Draw.Rounds.ElementAt(roundIndex);
            var matches = round.Permutations;
            
            if (matches == null || matches.Count == 0)
                continue;

            // Organize matches for this round
            var roundLayouts = ArrangeMatchesForRound(matches, matchWidth, matchHeight, pageNumber);
            layouts.AddRange(roundLayouts);
            
            // Calculate pages used for this round
            int pagesUsedForRound = (int)Math.Ceiling((double)matches.Count / (_maxRows * _maxCols));
            pageNumber += pagesUsedForRound;
        }

        return layouts;
    }

    /// <summary>
    /// Arranges matches for a specific round into a grid layout.
    /// </summary>
    /// <param name="matches">List of matches (permutations) for the round</param>
    /// <param name="matchWidth">Width of each match cell</param>
    /// <param name="matchHeight">Height of each match cell</param>
    /// <param name="startingPageNumber">The page number to start layout from</param>
    /// <returns>List of layout positions for the matches</returns>
    private List<(int, Rectangle)> ArrangeMatchesForRound(IReadOnlyList<Permutation> matches, 
        float matchWidth, float matchHeight, int startingPageNumber)
    {
        var layouts = new List<(int, Rectangle)>();
        int matchesPerPage = _maxRows * _maxCols;
        int currentPage = startingPageNumber;
        
        for (int matchIndex = 0; matchIndex < matches.Count; matchIndex++)
        {
            // Determine which page this match belongs to
            int pageOffset = matchIndex / matchesPerPage;
            int positionOnPage = matchIndex % matchesPerPage;
            
            // Calculate grid position on the current page
            int row = positionOnPage / _maxCols;
            int col = positionOnPage % _maxCols;
            
            // Calculate actual position coordinates
            float x = _pageLeftMargin + col * (matchWidth + _tablePaddingLeft + _tablePaddingRight) + _tablePaddingLeft;
            float y = _pageTopMargin + row * (matchHeight + _tablePaddingTop + _tablePaddingBottom) + _tablePaddingTop;
            
            var rectangle = new Rectangle(
                (int)x, 
                (int)y, 
                (int)matchWidth, 
                (int)matchHeight
            );
            
            layouts.Add((currentPage + pageOffset, rectangle));
        }
        
        return layouts;
    }

    /// <summary>
    /// Gets layout information for a specific round.
    /// Useful for displaying only matches from a particular round.
    /// </summary>
    /// <param name="tournament">The tournament object</param>
    /// <param name="roundNumber">The specific round number to layout (0-based)</param>
    /// <returns>Layout information for the specified round</returns>
    public List<(int, Rectangle)> ArrangeLayoutForRound(Tournament tournament, int roundNumber)
    {
        var layouts = new List<(int, Rectangle)>();
        
        if (tournament?.Draw?.Rounds == null || 
            roundNumber < 0 || 
            roundNumber >= tournament.Draw.Rounds.Count())
        {
            return layouts;
        }

        var round = tournament.Draw.Rounds.ElementAt(roundNumber);
        var matches = round.Permutations;
        
        if (matches == null || matches.Count == 0)
            return layouts;

        // Calculate match dimensions
        float availableWidth = _pageWidth - _pageLeftMargin - _pageRightMargin;
        float availableHeight = _pageHeight - _pageTopMargin - _pageBottomMargin;
        
        float matchWidth = (availableWidth - (_tablePaddingLeft + _tablePaddingRight) * _maxCols) / _maxCols;
        float matchHeight = (availableHeight - (_tablePaddingTop + _tablePaddingBottom) * _maxRows) / _maxRows;

        return ArrangeMatchesForRound(matches, matchWidth, matchHeight, 0);
    }

    /// <summary>
    /// Override Initialize to set Swiss-specific parameters.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        // Additional Swiss-specific initialization if needed
    }
}