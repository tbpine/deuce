using iText.Layout.Renderer;
using iText.Kernel.Colors;
using iText.Layout.Element;


class SizableCellRenderer : CellRenderer
{
    private readonly float[] _paddingsPer;
    public SizableCellRenderer(Cell cell, float[] paddingsPer) : base(cell)

    {
        _paddingsPer = paddingsPer;
    }

    public override void Draw(DrawContext drawContext)
    {
        base.Draw(drawContext);

        // Get the draw area (bounding box)
        var rect = this.GetOccupiedAreaBBox();
        // Example: Draw a red rectangle around the cell
        var canvas = drawContext.GetCanvas();
        canvas.SaveState();
        canvas.SetStrokeColor(ColorConstants.BLACK);

        //Work out padding sizes based  on percentage of width and height
        float paddingLeft = rect.GetWidth() * _paddingsPer[0];
        float paddingTop = rect.GetHeight() * _paddingsPer[1];
        float paddingRight = rect.GetWidth() * _paddingsPer[2];
        float paddingBottom = rect.GetHeight() * _paddingsPer[3];
        //Take pading
        canvas.Rectangle(rect.GetX() + paddingLeft,
         rect.GetY() + paddingTop,
          rect.GetWidth() - paddingRight - paddingLeft, rect.GetHeight() - paddingBottom - paddingTop);
        canvas.Stroke();
        canvas.RestoreState();

        // You can also log or use rect as needed
        System.Diagnostics.Debug.WriteLine($"Cell drawn at: {rect}");
    }
}