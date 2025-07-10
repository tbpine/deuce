using System.Drawing;

namespace deuce;


public class LayoutManagerTennisRR : LayoutManagerDefault
{
    // Use base class margin and padding members instead of redefining them here

    public LayoutManagerTennisRR(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
        : base(pageWidth, pageHeight, pageTopMargin, pageLeftMargin, pageRightMargin, pageBottomMargin, tablePaddingTop, tablePaddingBottom, tablePaddingLeft, tablePaddingRight)
    {
        // No need to assign margins/paddings here, use base class members
    }

    public override object ArrangeLayout(Tournament tournament)
    {
        return new List<(int, Rectangle)>();
    }
}