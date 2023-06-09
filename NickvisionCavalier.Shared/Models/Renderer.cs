using SkiaSharp;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    private DrawingDirection _direction => Configuration.Current.Direction;
    private float _offset => Configuration.Current.ItemsOffset;
    private float _roundness => Configuration.Current.ItemsRoundness;
    private bool _fill => Configuration.Current.Filling;
    private uint _thickness => Configuration.Current.LinesThickness;

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
        DrawBarsBox(sample, width, height);
        Canvas.Flush();
    }

    private void DrawBarsBox(float[] sample, float width, float height)
    {
        var paint = new SKPaint
        {
            Style = _fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            Color = SKColors.Blue
        };
        var step = (_direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (_direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRect(
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        0,
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        height * sample[i],
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRect(
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        height * (1 - sample[i]),
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        height * sample[i],
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRect(
                        0,
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        width * sample[i],
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRect(
                        width * (1 - sample[i]),
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        width * sample[i],
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        paint);
                    break;
            };
        }
    }
}
