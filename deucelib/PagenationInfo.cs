//Create a pagenation info class to manage pagination details
using System.Drawing;
namespace deuce;

public class PagenationInfo
{
    // Protected fields
    protected int _pageXIndex;
    protected int _pageYIndex;
    protected int _round;
    protected RectangleF _rectangle = new RectangleF();

    protected int _pagesX;
    protected int _pagesY;
    protected int _rowOffset;

    // Properties (single line)
    public int PageXIndex { get => _pageXIndex; set => _pageXIndex = value; }
    public int PageYIndex { get => _pageYIndex; set => _pageYIndex = value; }
    public int Round { get => _round; set => _round = value; }
    public RectangleF Rectangle { get => _rectangle; set => _rectangle = value; }

    public int PagesX { get => _pagesX; set => _pagesX = value; }
    public int PagesY { get => _pagesY; set => _pagesY = value; }
    public int RowOffset { get => _rowOffset; set => _rowOffset = value; }

    /// <summary>
    /// Default constructor for PagenationInfo.
    /// </summary>
    public PagenationInfo()
    {
    }

    /// <summary>
    /// Parameterized constructor for PagenationInfo.
    /// </summary>
    /// <param name="pageXIndex">The X index of the page.</param>
    /// <param name="pageYIndex">The Y index of the page.</param>
    /// <param name="round">The round number.</param>
    /// <param name="rectangle">The rectangle representing the layout area.</param>
    /// <param name="rowOffset">The row offset for the layout.</param>
    public PagenationInfo(int pageXIndex, int pageYIndex, int round, RectangleF rectangle, int rowOffset)
        : this()
    {
        _pageXIndex = pageXIndex;
        _pageYIndex = pageYIndex;
        _round = round;
        _rectangle = rectangle;
        _rowOffset = rowOffset;
    }
}