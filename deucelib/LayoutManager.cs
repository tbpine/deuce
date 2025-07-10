namespace deuce;

using System.Data;
using System.Drawing;
using DocumentFormat.OpenXml.Wordprocessing;

class LayoutManager
{
    private readonly float _page_top_margin;
    private readonly float _page_bottom_margin;
    private readonly float _page_left_margin;
    private readonly float _page_right_margin;
    private readonly float _table_padding_top;
    private readonly float _table_padding_bottom;
    private readonly float _table_padding_left;
    private readonly float _table_padding_right;
    private readonly float _pageWidth;
    private readonly float _pageHeight;

    public LayoutManager(float pageWidth, float pageHeight, float pageTopMargin,
    float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
    float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
    {
        _page_top_margin = pageTopMargin;
        _page_bottom_margin = pageBottomMargin;
        _page_left_margin = pageLeftMargin;
        _page_right_margin = pageRightMargin;
        _table_padding_top = tablePaddingTop;
        _table_padding_bottom = tablePaddingBottom;
        _pageWidth = pageWidth;
        _pageHeight = pageHeight;
        _table_padding_left = tablePaddingLeft;
        _table_padding_right = tablePaddingRight;
    }

/// <summary>
/// Calculate the layout for the given number of steps.
/// This method uses a ladder algorithm to determine the positions of rectangles
/// </summary>
/// <param name="steps"> The number of steps to calculate the layout for.
/// This is typically the number of matches in the first round of a tournament.</param>
/// <returns></returns>
    public List<(int, RectangleF)> Calculate(int steps, int rounds)
    {
        //the ladder algo
        //Work out the number of steps
        int totalCols = rounds; 

        //Work out the visible area of the page
        float visibleHeight = _pageHeight - _page_top_margin - _page_bottom_margin;
        float visibleWidth = _pageWidth - _page_left_margin - _page_right_margin;
        //Space evenly vertically
        float recHeight = (visibleHeight - steps * (_table_padding_top + _table_padding_bottom)) / steps;
        //Space evenly horizontally
        float recWidth = (visibleWidth - totalCols * (_table_padding_left + _table_padding_right)) / totalCols;
        // store the rectangles for each round  
        List<(int, RectangleF)> layout = new List<(int, RectangleF)>();
        //Column 1
        for (int i = 0; i < steps; i++)
        {
            RectangleF rect = new RectangleF(_page_left_margin,
                                             _page_top_margin + i * (recHeight + _table_padding_top + _table_padding_bottom),
                                              recWidth,
                                              recHeight);
            layout.Add((1, rect));

        }

        for (int r = 2; r <= totalCols; r++)
        {
            int stepHeight = steps / (int)Math.Pow(2, r - 1);
            //Rectangles are positioned in the middle of the mids of the previous
            //two rectangles.
            var prevSteps = layout.FindAll(x => x.Item1 == (r - 1));

            for (int j = 0; j < stepHeight; j++)
            {
                //Get two consecutive rectangles from the previous column
                if (j * 2 + 1 < prevSteps?.Count)
                {
                    var firstRect = prevSteps[j * 2].Item2;
                    var secondRect = prevSteps[j * 2 + 1].Item2;
                    
                    // Calculate the midpoint of each rectangle
                    float firstMid = firstRect.Top + (firstRect.Height / 2f);
                    float secondMid = secondRect.Top + (secondRect.Height / 2f);
                    
                    // Position the new rectangle at the midpoint between the two midpoints
                    float top = (firstMid + secondMid) / 2f - (recHeight / 2f);
                    
                    RectangleF rect = new RectangleF(_page_left_margin + (r - 1) * (recWidth + _table_padding_left),
                                                     top,
                                                     recWidth,
                                                     recHeight);
                    layout.Add((r, rect));
                }

            }

        }

        //Return the rectangles
        return layout;
    }
}