using SkiaSharp;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    private DrawingDirection _direction => Configuration.Current.Direction;
    private float _offset => Configuration.Current.ItemsOffset;
    private float _roundness => Configuration.Current.ItemsRoundness;
    private bool _fill => Configuration.Current.Filling;
    private float _thickness => (float)Configuration.Current.LinesThickness;

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
        var fgPaint = new SKPaint
        {
            Style = _fill ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            StrokeWidth = _thickness,
            Color = SKColors.Blue
        };
        switch (Configuration.Current.Mode)
        {
            case DrawingMode.WaveBox:
                DrawWaveBox(sample, width, height, fgPaint);
                break;
            case DrawingMode.BarsBox:
                DrawBarsBox(sample, width, height, fgPaint);
                break;
        }
        Canvas.Flush();
    }

    private void DrawWaveBox(float[] sample, float width, float height, SKPaint paint)
    {
        var step = (_direction < DrawingDirection.LeftRight ? width : height) / (sample.Length - 1);
        var path = new SKPath();
        switch (_direction)
        {
            case DrawingDirection.TopBottom:
                path.MoveTo(0, height * sample[0] - (_fill ? 0 : _thickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        step * (i + 0.5f),
                        height * sample[i] - (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        height * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        step * (i + 1),
                        height * sample[i+1] - (_fill ? 0 : _thickness / 2));
                }
                if (_fill)
                {
                    path.LineTo(width, 0);
                    path.LineTo(0, 0);
                    path.Close();
                }
                break;
            case DrawingDirection.BottomTop:
                path.MoveTo(0, height * (1 - sample[0]) + (_fill ? 0 : _thickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        step * (i + 0.5f),
                        height * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        height * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        step * (i + 1),
                        height * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2));
                }
                if (_fill)
                {
                    path.LineTo(width, height);
                    path.LineTo(0, height);
                    path.Close();
                }
                break;
            case DrawingDirection.LeftRight:
                path.MoveTo(width * sample[0] - (_fill ? 0 : _thickness / 2), 0);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        width * sample[i] - (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        width * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        width * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        step * (i + 1));
                }
                if (_fill)
                {
                    path.LineTo(0, height);
                    path.LineTo(0, 0);
                    path.Close();
                }
                break;
            case DrawingDirection.RightLeft:
                path.MoveTo(width * (1 - sample[0]) + (_fill ? 0 : _thickness / 2), 0);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        width * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        width * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        step * (i + 0.5f),
                        width * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        step * (i + 1));
                }
                if (_fill)
                {
                    path.LineTo(width, height);
                    path.LineTo(width, 0);
                    path.Close();
                }
                break;
        }
        Canvas.DrawPath(path, paint);
        path.Dispose();
    }

    private void DrawBarsBox(float[] sample, float width, float height, SKPaint paint)
    {
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
                        _fill ? 0 : _thickness / 2,
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        height * sample[i] - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRect(
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        height * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        height * sample[i] - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRect(
                        _fill ? 0 : _thickness / 2,
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        width * sample[i] - (_fill ? 0 : _thickness),
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRect(
                        width * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        step * (i + _offset / 2) + (_fill ? 0 : _thickness / 2),
                        width * sample[i] - (_fill ? 0 : _thickness),
                        step * (1 - _offset) - (_fill ? 0 : _thickness),
                        paint);
                    break;
            };
        }
    }
}
