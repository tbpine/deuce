
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Element;
using iText.Layout.Renderer;

namespace deuce;

public class ScoreBoxCellRenderer : CellRenderer
{
    public ScoreBoxCellRenderer(Cell cell) : base(cell)
    {

    }

    public override void DrawBorder(DrawContext drawContext)
    {
        Rectangle rectangle = this.GetOccupiedAreaBBox();

        float boxWidth = 10f;
        float boxHeight = 15f;

        PdfCanvas canvas = drawContext.GetCanvas();

        float x = rectangle.GetX() + (rectangle.GetWidth() - boxWidth) / 2f;
        float y = rectangle.GetY() +  (rectangle.GetHeight() - boxHeight) / 2f;
        Rectangle score = new Rectangle(x, y, boxWidth, boxHeight);

        canvas.SetStrokeColor(ColorConstants.BLACK);
        canvas.SetLineWidth(2);
        canvas.Rectangle(score);
        canvas.Stroke();

        base.DrawBorder(drawContext);
            
    }
}