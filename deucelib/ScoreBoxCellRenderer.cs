
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

    public override void Draw(DrawContext drawContext)
    {
        base.Draw(drawContext);
        
        if (!string.IsNullOrEmpty(_text))
        {
            Rectangle rectangle = this.GetOccupiedAreaBBox();
            PdfCanvas canvas = drawContext.GetCanvas();
            
            // Calculate the center of the drawn box
            float boxX = (rectangle.GetX() + (rectangle.GetWidth() - _boxWidth) / 2f);
            float boxY = (rectangle.GetY() + (rectangle.GetHeight() - _boxHeight) / 2f);
            float centerX = boxX + _boxWidth / 2f;
            float centerY = boxY + _boxHeight / 2f;
            
            // Create font and calculate appropriate font size based on box dimensions
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            
            // Calculate font size based on box height (with some padding)
            float maxFontSize = _boxHeight * 0.6f; // Use 60% of box height
            float fontSize = Math.Min(maxFontSize, 12f); // Cap at 12pt
            
            // Adjust font size to fit text width if necessary
            float textWidth = font.GetWidth(_text, fontSize);
            float maxTextWidth = _boxWidth * 0.8f; // Use 80% of box width
            
            if (textWidth > maxTextWidth)
            {
                fontSize = fontSize * (maxTextWidth / textWidth);
            }
            
            // Ensure minimum readable font size
            fontSize = Math.Max(fontSize, 4f);
            
            // Recalculate text dimensions with final font size
            textWidth = font.GetWidth(_text, fontSize);
            
            // Position text in center of the box
            float textX = centerX - textWidth / 2f;
            float textY = centerY - fontSize / 4f; // Adjust for baseline
            
            canvas.BeginText()
                  .SetFontAndSize(font, fontSize)
                  .SetTextMatrix(textX, textY)
                  .ShowText(_text)
                  .EndText();
        }
    }

}