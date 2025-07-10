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
    private readonly int _rows;
    private readonly int _cols;
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

    public List<(int, RectangleF)> Calculate(int steps)
    {
        //the ladder algo
        //Work out the number of steps
        int totalCols = (int)Math.Ceiling((double)steps / 2);

        //Work out the visible area of the page
        float visibleHeight = _pageHeight - _page_top_margin - _page_bottom_margin;
        float visibleWidth = _pageWidth - _page_left_margin - _page_right_margin;
        //Space evenly vertically
        float recHeight = (visibleHeight - steps * (_table_padding_top + _table_padding_bottom)) / steps;
        //Space evenly horizontally
        float recWidth = (visibleWidth - totalCols * (_table_padding_left + _table_padding_right)) / totalCols;
        //Set locations of 
        List<(int, RectangleF)> layout = new List<(int, RectangleF)>();
        //Column 1
        for (int i = 0; i < steps; i++)
        {
            RectangleF rect = new RectangleF(_page_left_margin,
                                             _page_top_margin + i * (recHeight + _table_padding_top + _table_padding_bottom),
                                              recWidth,
                                              recHeight);
            layout.Add( (1, rect));

        }
        
        for (int r = 2; r <= totalCols; r++)
        {
            int stepHeight = steps / (int)Math.Pow(2, r-1);
            //The next set of rectangles are vertically in the middle
            //of the previous column rectangles starting at the top
            var prevSteps = layout.FindAll(x => x.Item1 == (r - 1));

            for (int j = 0; j < stepHeight; j++)
            {
                //Get all rectangle from the first column
                if (j * 2 < prevSteps?.Count)
                {
                    var previousStep = prevSteps[j * 2].Item2;
                    float top = previousStep.Top + (previousStep.Height / 2f);
                    RectangleF rect = new RectangleF(_page_left_margin + (r-1) * (recWidth + _table_padding_left),
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