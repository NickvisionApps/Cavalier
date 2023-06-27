using SkiaSharp;
using System.Linq;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    private Mirror _mirror => Configuration.Current.Mirror;
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
                if (_mirror == Mirror.Full)
                {
                    DrawWaveBox(sample, _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawWaveBox(sample, GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else if (_mirror == Mirror.SplitChannels)
                {
                    DrawWaveBox(sample.Take(sample.Length / 2).ToArray(), _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawWaveBox(sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else
                {
                    DrawWaveBox(sample, _direction, 0, 0, width, height, fgPaint);
                }
                break;
            case DrawingMode.ParticlesBox:
                if (_mirror == Mirror.Full)
                {
                    DrawParticlesBox(sample, _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawParticlesBox(sample, GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else if (_mirror == Mirror.SplitChannels)
                {
                    DrawParticlesBox(sample.Take(sample.Length / 2).ToArray(), _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawParticlesBox(sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else
                {
                    DrawParticlesBox(sample, _direction, 0, 0, width, height, fgPaint);
                }
                break;
            case DrawingMode.BarsBox:
                if (_mirror == Mirror.Full)
                {
                    DrawBarsBox(sample, _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawBarsBox(sample, GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else if (_mirror == Mirror.SplitChannels)
                {
                    DrawBarsBox(sample.Take(sample.Length / 2).ToArray(), _direction, 0, 0, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                    DrawBarsBox(sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
                }
                else
                {
                    DrawBarsBox(sample, _direction, 0, 0, width, height, fgPaint);
                }
                break;
        }
        Canvas.Flush();
    }

    private DrawingDirection GetMirrorDirection()
    {
        return _direction switch
        {
            DrawingDirection.TopBottom => DrawingDirection.BottomTop,
            DrawingDirection.BottomTop => DrawingDirection.TopBottom,
            DrawingDirection.LeftRight => DrawingDirection.RightLeft,
            _ => DrawingDirection.LeftRight
        };
    }

    private float GetMirrorX(float width)
    {
        if (_direction == DrawingDirection.LeftRight || _direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f;
        }
        return 0;
    }

    private float GetMirrorY(float height)
    {
        if (_direction == DrawingDirection.TopBottom || _direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f;
        }
        return 0;
    }

    private float GetMirrorWidth(float width)
    {
        if (_direction == DrawingDirection.LeftRight || _direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f;
        }
        return width;
    }

    private float GetMirrorHeight(float height)
    {
        if (_direction == DrawingDirection.TopBottom || _direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f;
        }
        return height;
    }

    private void DrawWaveBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / (sample.Length - 1);
        var path = new SKPath();
        switch (direction)
        {
            case DrawingDirection.TopBottom:
                path.MoveTo(x, y + height * sample[0] - (_fill ? 0 : _thickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * sample[i] - (_fill ? 0 : _thickness / 2),
                        x + step * (i + 0.5f),
                        y + height * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        x + step * (i + 1),
                        y + height * sample[i+1] - (_fill ? 0 : _thickness / 2));
                }
                if (_fill)
                {
                    path.LineTo(x + width, y);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.BottomTop:
                path.MoveTo(x, y + height * (1 - sample[0]) + (_fill ? 0 : _thickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        x + step * (i + 1),
                        y + height * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2));
                }
                if (_fill)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x, y + height);
                    path.Close();
                }
                break;
            case DrawingDirection.LeftRight:
                path.MoveTo(x + width * sample[0] - (_fill ? 0 : _thickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * sample[i] - (_fill ? 0 : _thickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (_fill ? 0 : _thickness / 2),
                        y + step * (i + 1));
                }
                if (_fill)
                {
                    path.LineTo(x, y + height);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.RightLeft:
                path.MoveTo(x + width * (1 - sample[0]) + (_fill ? 0 : _thickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (_fill ? 0 : _thickness / 2),
                        y + step * (i + 1));
                }
                if (_fill)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x + width, y);
                    path.Close();
                }
                break;
        }
        Canvas.DrawPath(path, paint);
        path.Dispose();
    }

    private void DrawParticlesBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 11) * (1 - _offset * 2) - (_fill ? 0 : _thickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 11 : step) * (1 - _offset * 2) - (_fill ? 0 : _thickness / 2);
        for (var i = 0; i < sample.Length; i++)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRoundRect(
                        x + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        y + height / 11 * 10 * sample[i] + height / 11 * _offset + (_fill ? 0 : _thickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * _roundness, itemHeight / 2 * _roundness,
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRoundRect(
                        x + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        y + height / 11 * 10 * (1 - sample[i]) + height / 11 * _offset + (_fill ? 0 : _thickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * _roundness, itemHeight / 2 * _roundness,
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * sample[i] + width / 11 * _offset + (_fill ? 0 : _thickness / 2),
                        y + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * _roundness, itemHeight / 2 * _roundness,
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * (1 - sample[i]) + width / 11 * _offset + (_fill ? 0 : _thickness / 2),
                        y + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * _roundness, itemHeight / 2 * _roundness,
                        paint);
                    break;
            }
        }
    }

    private void DrawBarsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRect(
                        x + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        _fill ? y : y + _thickness / 2,
                        step * (1 - _offset * 2) - (_fill ? 0 : _thickness),
                        height * sample[i] - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRect(
                        x + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        y + height * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        step * (1 - _offset * 2) - (_fill ? 0 : _thickness),
                        height * sample[i] - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRect(
                        _fill ? x : x + _thickness / 2,
                        y + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        width * sample[i] - (_fill ? 0 : _thickness),
                        step * (1 - _offset * 2) - (_fill ? 0 : _thickness),
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRect(
                        x + width * (1 - sample[i]) + (_fill ? 0 : _thickness / 2),
                        y + step * (i + _offset) + (_fill ? 0 : _thickness / 2),
                        width * sample[i] - (_fill ? 0 : _thickness),
                        step * (1 - _offset * 2) - (_fill ? 0 : _thickness),
                        paint);
                    break;
            };
        }
    }
}
