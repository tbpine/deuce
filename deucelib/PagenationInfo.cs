//Create a pagenation info class to manage pagination details
using System.Drawing;
namespace deuce;

/// <summary>
/// Enum to specify the type of element this PagenationInfo represents
/// </summary>
public enum PageElementType
{
    Match,
    RoundHeader
}

public class PagenationInfo
{
    // Protected fields
    protected int _pageXIndex;
    protected int _pageYIndex;
    protected int _round;
    protected int _pageIndex;
    protected RectangleF _rectangle = new RectangleF();
    protected PageElementType _elementType = PageElementType.Match;
    protected string _text = "";

    protected int _pagesX;
    protected int _pagesY;
    protected int _rowOffset;

    protected bool _isPlayoffRound;

    // Properties (single line)
    public int PageXIndex { get => _pageXIndex; set => _pageXIndex = value; }
    public int PageYIndex { get => _pageYIndex; set => _pageYIndex = value; }
    public int Round { get => _round; set => _round = value; }
    public RectangleF Rectangle { get => _rectangle; set => _rectangle = value; }
    public PageElementType ElementType { get => _elementType; set => _elementType = value; }
    public string Text { get => _text; set => _text = value; }

    public int PagesX { get => _pagesX; set => _pagesX = value; }
    public int PagesY { get => _pagesY; set => _pagesY = value; }
    public int RowOffset { get => _rowOffset; set => _rowOffset = value; }

    public int PageIndex { get => _pageIndex; set => _pageIndex = value; }

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
    /// <param name="pageIndex"> The index of the page.</param>
    public PagenationInfo(int pageXIndex, int pageYIndex, int round, RectangleF rectangle, int rowOffset,
    int pageIndex = 1, bool isPlayoffRound = false)
        : this()
    {
        _pageXIndex = pageXIndex;
        _pageYIndex = pageYIndex;
        _round = round;
        _rectangle = rectangle;
        _rowOffset = rowOffset;
        _pageIndex = pageIndex;
        _elementType = PageElementType.Match;
        _isPlayoffRound = isPlayoffRound;
    }

    /// <summary>
    /// Constructor for header elements.
    /// </summary>
    /// <param name="pageXIndex">The X index of the page.</param>
    /// <param name="pageYIndex">The Y index of the page.</param>
    /// <param name="round">The round number.</param>
    /// <param name="rectangle">The rectangle representing the layout area.</param>
    /// <param name="text">The text to display in the header.</param>
    /// <param name="pageIndex">The index of the page.</param>
    /// <param name="elementType">The type of element this represents.</param>
    public PagenationInfo(int pageXIndex, int pageYIndex, int round, RectangleF rectangle, string text,
    int pageIndex = 1, PageElementType elementType = PageElementType.RoundHeader, bool isPlayoffRound = false)
        : this()
    {
        _pageXIndex = pageXIndex;
        _pageYIndex = pageYIndex;
        _round = round;
        _rectangle = rectangle;
        _text = text;
        _pageIndex = pageIndex;
        _elementType = elementType;
        _rowOffset = 0; // Not relevant for headers
        _isPlayoffRound = isPlayoffRound;
    }
}