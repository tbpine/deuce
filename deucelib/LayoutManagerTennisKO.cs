namespace deuce;

using System.Drawing;

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

    public override object ArrangeLayout(Tournament tournament)
    {
        //Define "steps" as the number of matches in the first round
        int steps = tournament.Schedule?.Rounds.FirstOrDefault(e => e.Index == 1)?.Permutations.Sum(e => e.Matches.Count) ?? 0;

        //the ladder algo
        //Work out the number of steps
        int totalCols = (int) Math.Log2(steps) + 1; // Total columns = log2(steps) + 1 for the first column
         //(int)Math.Ceiling((double)steps / 2) + 1;

        //Work out the visible area of the page
        float visibleHeight = _pageHeight - _pageTopMargin - _pageBottomMargin;
        float visibleWidth = _pageWidth - _pageLeftMargin - _pageRightMargin;
        //Space evenly vertically
        float recHeight = (visibleHeight - steps * (_tablePaddingTop + _tablePaddingBottom)) / steps;
        //Space evenly horizontally
        float recWidth = (visibleWidth - totalCols * (_tablePaddingLeft + _tablePaddingRight)) / totalCols;
        //Set locations of 
        List<(int, RectangleF)> layout = new List<(int, RectangleF)>();
        //Column 1
        for (int i = 0; i < steps; i++)
        {
            RectangleF rect = new RectangleF(_pageLeftMargin,
                                             _pageTopMargin + i * (recHeight + _tablePaddingTop + _tablePaddingBottom),
                                              recWidth,
                                              recHeight);
            layout.Add((1, rect));
        }

        for (int r = 2; r <= totalCols; r++)
        {
            int stepHeight = steps / (int)Math.Pow(2, r - 1);
            var prevSteps = layout.FindAll(x => x.Item1 == (r - 1));

            for (int j = 0; j < stepHeight; j++)
            {
                int idx1 = j * 2;
                int idx2 = idx1 + 1;
                if (prevSteps != null && idx2 < prevSteps.Count)
                {
                    var prevRect1 = prevSteps[idx1].Item2;
                    var prevRect2 = prevSteps[idx2].Item2;
                    float mid1 = prevRect1.Top + prevRect1.Height / 2f;
                    float mid2 = prevRect2.Top + prevRect2.Height / 2f;
                    float center = (mid1 + mid2) / 2f;
                    RectangleF rect = new RectangleF(
                        _pageLeftMargin + (r - 1) * (recWidth + _tablePaddingLeft),
                        center - recHeight / 2f,
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