
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout.Renderer;

namespace deuce;


public class FixedSizeCellRenderer : CellRenderer
{
    private string? _text;
    private float _boxWidth;
    private float _boxHeight;
    private float _lineWidth = 2f;



    public FixedSizeCellRenderer(Cell cell, string? text = "", float boxWidth = 10f, float boxHeight = 15f,
    float lineWidth = 2f) : base(cell)
    {
        _text = text;
        _boxWidth = boxWidth;
        _boxHeight = boxHeight;
        _lineWidth = lineWidth;
    }

    public override void DrawBorder(DrawContext drawContext)
    {
        Rectangle rectangle = this.GetOccupiedAreaBBox();

        PdfCanvas canvas = drawContext.GetCanvas();

        float x = (rectangle.GetX() + (rectangle.GetWidth() - _boxWidth) / 2f);
        float y = (rectangle.GetY() + (rectangle.GetHeight() - _boxHeight) / 2f);
        Rectangle score = new Rectangle(x, y, _boxWidth, _boxHeight);

        canvas.SetStrokeColor(ColorConstants.BLACK);
        canvas.SetLineWidth(_lineWidth);
        canvas.Rectangle(score);
        canvas.Stroke();

        base.DrawBorder(drawContext);
    }


}