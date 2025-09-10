namespace deuce;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

/// <summary>
/// Layout manager for Tennis Knockout with Playoff format.
/// Arranges matches in a knockout bracket style with support for playoff rounds.   
/// </summary>
public class LayoutManagerTennisKOPlayoff : LayoutManagerDefault
{
    /// <summary>
    /// Initializes a new instance of the LayoutManagerTennisKOPlayoff class.
    /// Sets up the layout manager with specified page dimensions, margins, and padding.    
    /// </summary>
    /// <param name="pageWidth">Width of the page.</param>
    /// <param name="pageHeight">Height of the page.</param>
    /// <param name="pageTopMargin">Top margin of the page.</param>
    /// <param name="pageLeftMargin">Left margin of the page.</param>
    /// <param name="pageRightMargin">Right margin of the page.</param>
    /// <param name="pageBottomMargin">Bottom margin of the page.</param>
    /// <param name="tablePaddingTop">Top padding for tables.</param>
    /// <param name="tablePaddingBottom">Bottom padding for tables.</param>
    /// <param name="tablePaddingLeft">Left padding for tables.</param>
    /// <param name="tablePaddingRight">Right padding for tables.</param>
    /// <remarks>
    /// This constructor initializes the layout manager with the provided page dimensions and margin/padding settings.
    /// </remarks>
    public LayoutManagerTennisKOPlayoff(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
        : base(pageWidth, pageHeight, pageTopMargin, pageLeftMargin, pageRightMargin, pageBottomMargin, tablePaddingTop, tablePaddingBottom, tablePaddingLeft, tablePaddingRight)
    {
        // No need to assign margins/paddings here, use base class members
    }

    /// <summary>
    /// Arranges the layout for a Tennis Knockout with Playoff tournament.
    /// Generates a list of PagenationInfo objects representing the layout of matches across pages.
    /// </summary>
    /// <param name="tournament">The tournament to arrange the layout for.</param>
    /// <returns>A list of PagenationInfo objects representing the layout.</returns>
    public override object ArrangeLayout(Tournament tournament)
    {
        //Define the rules for each page:
        //Has max _maxcols number of columns
        //has max _maxrows of games
        //Reserve room at the top for headers

        //Work out how many blocks of columns are required
        //For each block, create pages for the main round
        //and the playoff round


        //Assumption: every thing is a multiple of 2.

        //Define "totalMatches" as the number of matches in the first round
        int totalMatches = tournament.Draw?.Rounds.FirstOrDefault(e => e.Index == 1)?.Permutations.Sum(e => e.Matches.Count) ?? 0;

        //the ladder algo
        //1 column for each round, 
        int totalCols = (int)Math.Log2(totalMatches) + 1;
        //Work out how many pages are needed with blocks of rounds
        //But, make each page fit one round
        int blocsOfCols = (int)Math.Ceiling((double)totalCols / _maxCols);
        //Save page index
        int pageIndex = 1;
        //Debug out
        Console.WriteLine($"Total Matches: {totalMatches}, Total Columns: {totalCols}, Blocks of Columns: {blocsOfCols}");

        List<PagenationInfo> layout = new List<PagenationInfo>();
        //Blocks of cols 
        //Main round pages
        for (int i = 0; i < blocsOfCols; i++)
        {
            int startRound = i * _maxCols; // zero indexed
            int endRound = Math.Min(startRound + _maxCols - 1, totalCols - 1);
            //Find the number of matches in the round
            int matchesInRound = (int)(totalMatches / Math.Pow(2, startRound));
            //Work out how many pages needed for this block
            int totalPages = (int)Math.Ceiling((double)matchesInRound / _maxRows);
            int matchesPerPage = Math.Min(matchesInRound, _maxRows);
            //Find the round range for this block

            for (int j = 0; j < totalPages; j++)
            {
                //call "ArrangePageLayout" to arrange the layout for this page
                //Pass the layout, start round, end round, and page index
                ArrangePageLayout(layout, startRound, endRound, pageIndex, matchesPerPage, totalMatches, j);
                pageIndex++;
            }
        }
        //Do the playoff round
        //Define "totalMatches" as the number of matches in the first round
        totalMatches = tournament.Draw?.Rounds.FirstOrDefault(e => e.Index == 1)?.Playoff?.Permutations.Sum(e => e.Matches.Count) ?? 0;

        //the ladder algo
        //Work out the number of steps
        totalCols = (int)Math.Log2(totalMatches) + 1;
        //Work out how many pages are needed with blocks of rounds
        blocsOfCols = (int)Math.Ceiling((double)totalCols / _maxCols);

        for (int i = 0; i < blocsOfCols; i++)
        {
            //find the round range for this block
            int startRound = i * _maxCols; // zero indexed
            int endRound = Math.Min(startRound + _maxCols - 1, totalCols);
            //Find the number of matches for the start round of this block
            int matchesInStartRound = (int)(totalMatches / Math.Pow(2, startRound));
            //Work out how many pages needed for this block
            int totalPages = (int)Math.Ceiling((double)matchesInStartRound / _maxRows);
            int matchesPerPage = Math.Min(matchesInStartRound, _maxRows);

            for (int j = 0; j < totalPages; j++)
            {
                //call "ArrangePageLayout" to arrange the layout for this page
                //Pass the layout, start round, end round, and page index
                ArrangePageLayout(layout, startRound, endRound, pageIndex, matchesPerPage, totalMatches, j, true);
                pageIndex++;
            }
        }
        //Last page is for the winner of the main round and playoff round
        int lastRoundIdx = (tournament?.Draw?.Rounds.Count() ?? 0) - 1;
        ArrangePageLayout(layout, lastRoundIdx, lastRoundIdx, pageIndex, 1, 1, 0, false, true);


        //Return layout
        return layout;
    }

    /// <summary>
    /// Arranges the layout for a single page in the Tennis Knockout with Playoff format.
    /// Adds PagenationInfo objects to the provided layout list for matches and headers.    
    /// </summary>
    /// <param name="layout">The list to add PagenationInfo objects to.</param>
    /// <param name="startRound">The starting round index for this page (0-based).</param>
    /// <param name="endRound">The ending round index for this page (0-based).</param>
    /// <param name="pageIndex">The index of the current page (1-based).</param>
    /// <param name="rowsInFirstColumn">The number of match rows in the first column.</param>
    /// <param name="totalMatches">The total number of matches in the first round.</param>
    /// <param name="pageInRound">The index of the current page within the round (0-based).</param>
    /// <param name="isPlayoffRound">Indicates if this layout is for the playoff round.</param>
    /// <param name="isFinalMatch">Indicates if this layout is for the final match.</param>
    private void ArrangePageLayout(List<PagenationInfo> layout, int startRound, int endRound, int pageIndex,
     int rowsInFirstColumn, int totalMatches, int pageInRound, bool isPlayoffRound = false, bool isFinalMatch = false)
    {
        //Work out the visible area of the page
        //Create a RectangleF for the draw area per page
        RectangleF drawArea = new RectangleF(_pageLeftMargin, _pageTopMargin,
            _pageWidth - _pageLeftMargin - _pageRightMargin,
            _pageHeight - _pageTopMargin - _pageBottomMargin);

        // Reserve space at the top for round headers
        float headerHeight = 30f; // Height for round headers
        float headerSpacing = 5f; // Spacing between headers and content
        float contentStartY = _pageTopMargin + headerHeight + headerSpacing;

        // Adjust draw area to account for header space
        RectangleF contentArea = new RectangleF(_pageLeftMargin, contentStartY,
            _pageWidth - _pageLeftMargin - _pageRightMargin,
            _pageHeight - contentStartY - _pageBottomMargin);

        //Space evenly vertically in the content area
        float recHeight = (contentArea.Height - _maxRows * (_tablePaddingTop + _tablePaddingBottom)) / _maxRows;
        //Space evenly horizontally
        float recWidth = (drawArea.Width - _maxCols * (_tablePaddingLeft + _tablePaddingRight)) / _maxCols;

        // Add round label rectangle in the middle of the header
        float labelWidth = 120f;
        float labelHeight = headerHeight; // Use the same height as the header
        string labelText = "";
        RectangleF labelRect = new RectangleF(
            (_pageWidth - labelWidth) / 2f, // Center horizontally
            _pageTopMargin, // Position at the top margin (same as headers)
            labelWidth,
            labelHeight);
        layout.Add(new PagenationInfo(0, 0, 0, labelRect, labelText, pageIndex, PageElementType.RoundLabel, isPlayoffRound) { IsFinalMatch = isFinalMatch });

        // Add round headers at the top of the page
        for (int r = 0; r <= (endRound - startRound); r++)
        {
            int currentRound = startRound + r; // Convert to 1-indexed round
            int matchesInRound = totalMatches / (int)Math.Pow(2, r);
            //Calculate number of players
            int noPlayers = matchesInRound * 2; // Each match has two players
            string subSectionText = isPlayoffRound ? "Playoff" : "Main";
            string headerText = isFinalMatch ? "Final" : $"Round {currentRound + 1} ({subSectionText})"; // Number of matches times 2

            // if (noPlayers == 2) headerText = "Final"; // Special case for final round
            // else if (noPlayers == 4) headerText = "Semi-Final"; // Special case for semi-finals
            // else if (noPlayers == 8) headerText = "Quarter-Final"; // Special case for quarter-finals

            RectangleF headerRect = new RectangleF(
                _pageLeftMargin + r * (recWidth + _tablePaddingLeft),
                _pageTopMargin,
                recWidth,
                headerHeight);

            layout.Add(new PagenationInfo(0, 0, currentRound, headerRect, headerText, pageIndex, PageElementType.RoundHeader, isPlayoffRound) { IsFinalMatch = isFinalMatch });
        }

        float totalFirstRowHeight = rowsInFirstColumn * (recHeight + _tablePaddingTop + _tablePaddingBottom);
        float startY = contentStartY + (contentArea.Height - totalFirstRowHeight) / 2f;

        for (int i = 0; i < rowsInFirstColumn; i++)
        {
            RectangleF rect = new RectangleF(_pageLeftMargin,
                                             startY + i * (recHeight + _tablePaddingTop + _tablePaddingBottom),
                                              recWidth,
                                              recHeight);
            //Work out the row position of the match in the row
            int rowOffset = pageInRound * _maxRows + i;
            layout.Add(new PagenationInfo(0, 0, startRound + 1, rect, rowOffset, pageIndex, isPlayoffRound) { IsFinalMatch = isFinalMatch });
        }

        //Columns passed the first
        for (int r = 1; r <= (endRound - startRound); r++)
        {
            int rows = rowsInFirstColumn / (int)Math.Pow(2, r);
            //Find the previous round
            int prevRound = startRound + r;
            //Find layouts from the previous round that is not a header

            var prevSteps = layout.FindAll(x => x.Round == prevRound && x.ElementType != PageElementType.RoundHeader
            && x.IsPlayoffRound == isPlayoffRound);

            //Number of rows per page
            for (int j = 0; j < rows; j++)
            {
                //The rectangle location is between the previous two rectangles
                //in the previous round.
                int idx1 = j * 2;
                int idx2 = idx1 + 1;
                if (prevSteps != null && idx2 < prevSteps.Count)
                {
                    //Calculate the rectangle position based on the previous rectangles
                    //and the current round's rectangle width and height.

                    var prevRect1 = prevSteps[idx1].Rectangle;
                    var prevRect2 = prevSteps[idx2].Rectangle;
                    float mid1 = prevRect1.Top + prevRect1.Height / 2f;
                    float mid2 = prevRect2.Top + prevRect2.Height / 2f;
                    //Calculate the center position for the new rectangle
                    float center = (mid1 + mid2) / 2f;
                    //Create the new rectangle for the current match
                    RectangleF rect = new RectangleF(
                         _pageLeftMargin + r * (recWidth + _tablePaddingLeft),
                        center - recHeight / 2f,
                        recWidth,
                        recHeight);
                    //For the round , work out the number of matches per page
                    int matchesPerPage = (int)Math.Ceiling((float)rowsInFirstColumn / (float)Math.Pow(2, r));
                    matchesPerPage = Math.Min(matchesPerPage, _maxRows);
                    int rowOffset = pageInRound * matchesPerPage + j;
                    layout.Add(new PagenationInfo(0, 0, startRound + r + 1, rect, rowOffset,
                        pageIndex, isPlayoffRound)
                    { IsFinalMatch = isFinalMatch });
                }
            }
        }


    }

    /// <summary>
    /// Generates a debug string listing all round headers and labels in the layout.
    /// </summary>
    /// <param name="layout"></param>
    /// <returns></returns>
    public static string DebugHeaders(List<PagenationInfo> layout)
    {
        var headers = layout.Where(p => p.ElementType == PageElementType.RoundHeader).ToList();
        var labels = layout.Where(p => p.ElementType == PageElementType.RoundLabel).ToList();
        var result = $"Found {headers.Count} round headers and {labels.Count} round labels:\n";

        foreach (var header in headers)
        {
            result += $"Round Header {header.Round}: '{header.Text}' at ({header.Rectangle.X}, {header.Rectangle.Y})\n";
        }

        foreach (var label in labels)
        {
            result += $"Round Label on Page {label.PageIndex}: '{label.Text}' at ({label.Rectangle.X}, {label.Rectangle.Y})\n";
        }

        return result;
    }
}