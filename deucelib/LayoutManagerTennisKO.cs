namespace deuce;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
/// <summary>
/// Layout manager for Tennis Knockout tournaments.
/// This class extends the default layout manager to specifically handle the layout of a Tennis Knockout tournament bracket.
/// It calculates the layout based on the number of steps (matches) and the page dimensions.
/// It arranges the rectangles for each match in a knockout format, ensuring proper spacing and alignment.
/// The layout is designed to fit within the specified page margins and paddings.
/// </summary>
public class LayoutManagerTennisKO : LayoutManagerDefault
{
    // Use base class margin and padding members instead of redefining them here
    public LayoutManagerTennisKO(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
        : base(pageWidth, pageHeight, pageTopMargin, pageLeftMargin, pageRightMargin, pageBottomMargin, tablePaddingTop, tablePaddingBottom, tablePaddingLeft, tablePaddingRight)
    {
        // No need to assign margins/paddings here, use base class members
    }

    /// <summary>
    /// Arranges the layout for a Tennis Knockout tournament.
    /// This method calculates the layout based on the number of matches in the first round,
    /// determines the number of pages needed, and arranges the matches in a knockout format.
    /// It returns a list of PagenationInfo objects that contain the layout information for each match rectangle.
    /// </summary>
    /// <param name="tournament"> The tournament object containing the schedule and match information.</param>
    /// <returns> A list of PagenationInfo objects representing the layout of the tournament matches.</returns>
    public override object ArrangeLayout(Tournament tournament)
    {
        //Assumption: every thing is a multiple of 2.

        //Define "steps" as the number of matches in the first round
        int totalMatches = tournament.Schedule?.Rounds.FirstOrDefault(e => e.Index == 1)?.Permutations.Sum(e => e.Matches.Count) ?? 0;

        //the ladder algo
        //Work out the number of steps
        int totalCols = (int)Math.Log2(totalMatches) + 1;

        //Calculate number of pages in the x direction
        int pagesX = totalCols / _maxCols + (totalCols % _maxCols > 0 ? 1 : 0);
        int pagesY = totalMatches / _maxRows + (totalMatches % _maxRows > 0 ? 1 : 0);


        //Go through each page from left to right,
        //Work out the matches on that page
        //Layout 
        //Then, go through each page from top to bottom
        //Find matches on that page
        //Layout the matches on that page
        List<PagenationInfo> layout = new List<PagenationInfo>();
        for (int y = 0; y < pagesY; y++)
        {
            for (int x = 0; x < pagesX; x++)
            {
                //Work out the number of columns in this page

                ArrangePageLayout(layout, x, y, totalMatches);
            }
        }

        //Set the pagesX and pagesY for each PagenationInfo object
        //This is useful for pagination purposes, especially when rendering the PDF
        foreach (PagenationInfo e in layout)
        {
            e.PagesX = pagesX;
            e.PagesY = pagesY;
        }

        //Return the rectangles
        return layout;
    }

    /// <summary>
    /// Arranges the layout for a specific page in the tournament bracket.
    /// </summary>
    /// <param name="layout"> The list to which the layout information will be added.</param>
    /// <param name="pageXIndex"> The index of the page in the X direction.</param>
    /// <param name="pageYIndex">  The index of the page in the Y direction.</param>
    private void ArrangePageLayout(List<PagenationInfo> layout, int pageXIndex, int pageYIndex,
    int totalMatches)
    {
        //Work out how many rows on this page. That is, 
        //what round in the tournament this page is.
        int startRound = pageXIndex * _maxCols;
        int rowsInFirstColumn = (int)(totalMatches / Math.Pow(2,startRound));

        //Work out the visible area of the page
        //Create a RectangleF for the draw area per page
        RectangleF drawArea = new RectangleF(_pageLeftMargin, _pageTopMargin,
            _pageWidth - _pageLeftMargin - _pageRightMargin,
            _pageHeight - _pageTopMargin - _pageBottomMargin);

        // Calculate page offsets for current page position
        float pageOffsetX = pageXIndex * _pageWidth;
        float pageOffsetY = pageYIndex * _pageHeight;

        //Space evenly vertically
        float recHeight = (drawArea.Height - _maxRows * (_tablePaddingTop + _tablePaddingBottom)) / _maxRows;
        //Space evenly horizontally
        float recWidth = (drawArea.Width - _maxCols * (_tablePaddingLeft + _tablePaddingRight)) / _maxCols;

        float totalFirstRowHeight = rowsInFirstColumn * (recHeight + _tablePaddingTop + _tablePaddingBottom);
        float startY = _pageTopMargin + (drawArea.Height - totalFirstRowHeight) / 2f;
        
        for (int i = 0; i < rowsInFirstColumn; i++)
        {
            RectangleF rect = new RectangleF(pageOffsetX + _pageLeftMargin,
                                             pageOffsetY + _pageTopMargin + i * (recHeight + _tablePaddingTop + _tablePaddingBottom)  + startY,
                                              recWidth,
                                              recHeight);
            //startRound is zero indexed, so we add 1 to it
            layout.Add(new PagenationInfo(pageXIndex, pageYIndex, startRound + 1, rect, i));
        }
        //Calcuate total number of rounds
        int totalRounds = (int)Math.Log2(totalMatches) + 1;

        int noRoundsAfter = totalRounds - (startRound+1) > _maxCols ? _maxCols : totalRounds - (startRound+1);

        //first round on the page is zero, so we start at 1
        for (int r = 1; r < noRoundsAfter; r++)
        {
            int rows = rowsInFirstColumn / (int)Math.Pow(2, r);
            //Find the previous round
            int prevRound = startRound + r;
            var prevSteps = layout.FindAll(x => x.Round == prevRound);

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
                        pageOffsetX + _pageLeftMargin + r  * (recWidth + _tablePaddingLeft),
                        center - recHeight / 2f,
                        recWidth,
                        recHeight);
                    layout.Add(new PagenationInfo(pageXIndex, pageYIndex, startRound + r + 1, rect, j));
                }
            }
        }

        
    }
}