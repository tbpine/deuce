namespace deuce;

using System.Drawing;

/// <summary>
/// Default implementation of ILayoutManager with protected margin properties.
/// </summary>
public class LayoutManagerDefault : ILayoutManager
{
    protected float _pageTopMargin;
    protected float _pageBottomMargin;
    protected float _pageLeftMargin;
    protected float _pageRightMargin;
    protected float _tablePaddingTop;
    protected float _tablePaddingBottom;
    protected float _tablePaddingLeft;
    protected float _tablePaddingRight;
    protected float _pageWidth;
    protected float _pageHeight;

    public LayoutManagerDefault(float pageWidth, float pageHeight, float pageTopMargin,
        float pageLeftMargin, float pageRightMargin, float pageBottomMargin, float tablePaddingTop, float tablePaddingBottom,
        float tablePaddingLeft = 5f, float tablePaddingRight = 5f)
    {
        _pageTopMargin = pageTopMargin;
        _pageBottomMargin = pageBottomMargin;
        _pageLeftMargin = pageLeftMargin;
        _pageRightMargin = pageRightMargin;
        _tablePaddingTop = tablePaddingTop;
        _tablePaddingBottom = tablePaddingBottom;
        _pageWidth = pageWidth;
        _pageHeight = pageHeight;
        _tablePaddingLeft = tablePaddingLeft;
        _tablePaddingRight = tablePaddingRight;
    }

    public virtual void Initialize()
    {
        // Initialization logic if needed
    }

    public virtual object ArrangeLayout(Tournament tournament)
    {
        // throw exception if not implemented
        throw new NotImplementedException("ArrangeLayout method is not implemented in LayoutManagerDefault.");
        
    }
}
