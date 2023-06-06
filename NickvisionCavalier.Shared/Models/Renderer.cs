using SkiaSharp;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    public SKCanvas? Canvas { get; set; }
    
    public Renderer()
    {
        Canvas = null;
    }
    
    public void Draw(float[] sample, float width, float height)
    {
        if (Canvas == null)
        {
            return;
        }
        Canvas.Clear();
        var paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Blue
        };
        var step = width / sample.Length;
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            Canvas.DrawRect(step * (i + 0.1f), height * (1 - sample[i]), step * 0.8f, height * sample[i], paint);
        }
        Canvas.Flush();
    }
}
