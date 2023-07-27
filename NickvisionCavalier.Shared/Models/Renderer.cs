using Nickvision.Aura;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace NickvisionCavalier.Shared.Models;

public class Renderer
{
    private delegate void DrawFunc(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint);
    private DrawFunc? _drawFunc;
    private int _imageIndex;
    private SKBitmap? _imageBitmap;
    private SKBitmap? _targetBitmap;
    private float _oldWidth;
    private float _oldHeight;

    public SKCanvas? Canvas { get; set; }
    
    public Renderer()
    {
        Canvas = null;
        _imageIndex = -1;
        _oldWidth = 0;
        _oldHeight = 0;
    }
    
    public void Draw(float[] sample, float width, float height)
    {
        if (Canvas == null)
        {
            return;
        }
        Canvas.Clear();
        var profile = Configuration.Current.ColorProfiles[Configuration.Current.ActiveProfile];
        // Draw background
        var bgPaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            IsAntialias = true
        };
        if (profile.BgColors.Count > 1)
        {
            bgPaint.Shader = CreateGradient(profile.BgColors, width, height);
        }
        else
        {
            bgPaint.Color = SKColor.Parse(profile.BgColors[0]);
        }
        Canvas.DrawRect(0, 0, width, height, bgPaint);
        // Draw image
        if (_imageIndex != Configuration.Current.ImageIndex)
        {
            if (Configuration.Current.ImageIndex == -1)
            {
                _imageBitmap?.Dispose();
                _targetBitmap?.Dispose();
            }
            else
            {
                var images = new List<string>();
                foreach (var file in Directory.GetFiles($"{ConfigLoader.ConfigDir}{Path.DirectorySeparatorChar}images"))
                {
                    if (file.EndsWith(".jpg") || file.EndsWith(".jpeg") || file.EndsWith(".png"))
                    {
                        images.Add(file);
                    }
                }
                images.Sort();
                if (Configuration.Current.ImageIndex < images.Count)
                {
                    _imageBitmap = SKBitmap.Decode(images[Configuration.Current.ImageIndex]);
                    _oldWidth = 0; // To enforce redraw
                    _oldHeight = 0;
                }
                else
                {
                    Configuration.Current.ImageIndex = -1;
                }
            }
            _imageIndex = Configuration.Current.ImageIndex;
        }
        if (_imageIndex != -1)
        {
            if ((int)_oldWidth != (int)width || (int)_oldHeight != (int)height)
            {
                _oldWidth = width;
                _oldHeight = height;
                var scale = Math.Max(width / _imageBitmap!.Width, height / _imageBitmap.Height);
                var rect = new SKRect(0, 0, _imageBitmap.Width * scale, _imageBitmap.Height * scale);
                _targetBitmap?.Dispose();
                _targetBitmap = new SKBitmap((int)rect.Width, (int)rect.Height);
                _imageBitmap.ScalePixels(_targetBitmap, SKFilterQuality.Medium);
            }
            Canvas.DrawBitmap(_targetBitmap, width / 2 - _targetBitmap!.Width / 2f, height / 2 - _targetBitmap.Height / 2f);
        }
        // Draw foreground
        width -= Configuration.Current.AreaMargin * 2;
        height -= Configuration.Current.AreaMargin * 2;
        var fgPaint = new SKPaint
        {
            Style = Configuration.Current.Filling ? SKPaintStyle.Fill : SKPaintStyle.Stroke,
            StrokeWidth = Configuration.Current.LinesThickness,
            IsAntialias = true
        };
        if (profile.FgColors.Count > 1 && Configuration.Current.Mode != DrawingMode.SpineBox)
        {
            fgPaint.Shader = CreateGradient(profile.FgColors, width, height, Configuration.Current.AreaMargin);
        }
        else
        {
            fgPaint.Color = SKColor.Parse(profile.FgColors[0]);
        }
        _drawFunc = Configuration.Current.Mode switch
        {
            DrawingMode.LevelsBox => DrawLevelsBox,
            DrawingMode.ParticlesBox => DrawParticlesBox,
            DrawingMode.BarsBox => DrawBarsBox,
            DrawingMode.SpineBox => DrawSpineBox,
            _ => DrawWaveBox,
        };
        if (Configuration.Current.Mirror == Mirror.Full)
        {
            _drawFunc(sample, Configuration.Current.Direction, Configuration.Current.AreaMargin, Configuration.Current.AreaMargin, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
            _drawFunc(Configuration.Current.ReverseMirror ? sample.Reverse().ToArray() : sample, GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
        }
        else if (Configuration.Current.Mirror == Mirror.SplitChannels)
        {
            _drawFunc(sample.Take(sample.Length / 2).ToArray(), Configuration.Current.Direction, Configuration.Current.AreaMargin, Configuration.Current.AreaMargin, GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
            _drawFunc(Configuration.Current.ReverseMirror ? sample.Skip(sample.Length / 2).ToArray() : sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(), GetMirrorX(width), GetMirrorY(height), GetMirrorWidth(width), GetMirrorHeight(height), fgPaint);
        }
        else
        {
            _drawFunc(sample, Configuration.Current.Direction, Configuration.Current.AreaMargin, Configuration.Current.AreaMargin, width, height, fgPaint);
        }
        Canvas.Flush();
    }

    /// <summary>
    /// Creates gradient shader
    /// </summary>
    /// <param name="colorStrings">List of colors as strings</param>
    /// <param name="width">Canvas width</param>
    /// <param name="height">Canvas height</param>
    /// <returns>Skia Shader</returns>
    private SKShader CreateGradient(List<string> colorStrings, float width, float height, uint margin = 0)
    {
        var colors = colorStrings.Select(c => SKColor.Parse(c)).Reverse().ToArray();
        if (Configuration.Current.Mirror != Mirror.Off)
        {
            var mirrorColors = new SKColor[colors.Length * 2];
            if (Configuration.Current.Direction == DrawingDirection.BottomTop || Configuration.Current.Direction == DrawingDirection.RightLeft)
            {
                Array.Reverse(colors);
            }
            colors.CopyTo(mirrorColors, 0);
            Array.Reverse(colors);
            colors.CopyTo(mirrorColors, colors.Length);
            colors = mirrorColors;
        }
        return Configuration.Current.Direction switch
        {
            DrawingDirection.TopBottom => SKShader.CreateLinearGradient(new SKPoint(margin, margin), new SKPoint(margin, height), colors, SKShaderTileMode.Clamp),
            DrawingDirection.BottomTop => SKShader.CreateLinearGradient(new SKPoint(margin, height), new SKPoint(margin, margin), colors, SKShaderTileMode.Clamp),
            DrawingDirection.LeftRight => SKShader.CreateLinearGradient(new SKPoint(margin, margin), new SKPoint(width, margin), colors, SKShaderTileMode.Clamp),
            _ => SKShader.CreateLinearGradient(new SKPoint(width, margin), new SKPoint(margin, margin), colors, SKShaderTileMode.Clamp)
        };
    }

    private DrawingDirection GetMirrorDirection()
    {
        return Configuration.Current.Direction switch
        {
            DrawingDirection.TopBottom => DrawingDirection.BottomTop,
            DrawingDirection.BottomTop => DrawingDirection.TopBottom,
            DrawingDirection.LeftRight => DrawingDirection.RightLeft,
            _ => DrawingDirection.LeftRight
        };
    }

    private float GetMirrorX(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f + Configuration.Current.AreaMargin;
        }
        return Configuration.Current.AreaMargin;
    }

    private float GetMirrorY(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f + Configuration.Current.AreaMargin;
        }
        return Configuration.Current.AreaMargin;
    }

    private float GetMirrorWidth(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2.0f;
        }
        return width;
    }

    private float GetMirrorHeight(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2.0f;
        }
        return height;
    }

    private void DrawWaveBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / (sample.Length - 1);
        using var path = new SKPath();
        switch (direction)
        {
            case DrawingDirection.TopBottom:
                path.MoveTo(x, y + height * sample[0] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 0.5f),
                        y + height * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 1),
                        y + height * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.BottomTop:
                path.MoveTo(x, y + height * (1 - sample[0]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 0.5f),
                        y + height * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + 1),
                        y + height * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x, y + height);
                    path.Close();
                }
                break;
            case DrawingDirection.LeftRight:
                path.MoveTo(x + width * sample[0] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * sample[i+1] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 1));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x, y + height);
                    path.LineTo(x, y);
                    path.Close();
                }
                break;
            case DrawingDirection.RightLeft:
                path.MoveTo(x + width * (1 - sample[0]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2), y);
                for (var i = 0; i < sample.Length - 1; i++)
                {
                    path.CubicTo(
                        x + width * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 0.5f),
                        x + width * (1 - sample[i+1]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + 1));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y + height);
                    path.LineTo(x + width, y);
                    path.Close();
                }
                break;
        }
        Canvas.DrawPath(path, paint);
    }

    private void DrawLevelsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 10) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 10 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        for (var i = 0; i < sample.Length; i++)
        {
            for (var j = 0; j < Math.Floor(sample[i] * 10); j++)
            {
                switch (direction)
                {
                    case DrawingDirection.TopBottom:
                        Canvas.DrawRoundRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 10 * j + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.BottomTop:
                        Canvas.DrawRoundRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 10 * (9 - j) + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.LeftRight:
                        Canvas.DrawRoundRect(
                            x + width / 10 * j + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                    case DrawingDirection.RightLeft:
                        Canvas.DrawRoundRect(
                            x + width / 10 * (9 - j) + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            itemWidth, itemHeight,
                            itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                            paint);
                        break;
                }
            }
        }
    }

    private void DrawParticlesBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 11) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 11 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        for (var i = 0; i < sample.Length; i++)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    Canvas.DrawRoundRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height / 11 * 10 * sample[i] + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRoundRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height / 11 * 10 * (1 - sample[i]) + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * sample[i] + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRoundRect(
                        x + width / 11 * 10 * (1 - sample[i]) + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        itemWidth, itemHeight,
                        itemWidth / 2 * Configuration.Current.ItemsRoundness, itemHeight / 2 * Configuration.Current.ItemsRoundness,
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
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        (Configuration.Current.Filling ? y : y + Configuration.Current.LinesThickness / 2) - 1,
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) + 1,
                        paint);
                    break;
                case DrawingDirection.BottomTop:
                    Canvas.DrawRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) + 1,
                        paint);
                    break;
                case DrawingDirection.LeftRight:
                    Canvas.DrawRect(
                        Configuration.Current.Filling ? x : x + Configuration.Current.LinesThickness / 2,
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
                case DrawingDirection.RightLeft:
                    Canvas.DrawRect(
                        x + width * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        paint);
                    break;
            };
        }
    }

    private void DrawSpineBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemSize = step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness);
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                case DrawingDirection.BottomTop:
                    Canvas.DrawRoundRect(
                        x + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        y + height / 2 - itemSize * sample[i] / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        GetSpinePaint(paint, sample[i]));
                    break;
                case DrawingDirection.LeftRight:
                case DrawingDirection.RightLeft:
                    Canvas.DrawRoundRect(
                        x + width / 2 - itemSize * sample[i] / 2,
                        y + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        GetSpinePaint(paint, sample[i]));
                    break;
            }
        }
    }

    /// <summary>
    /// Sets paint color for Spine element
    /// </summary>
    /// <param name="paint">Skia paint</param>
    /// <param name="sample">CAVA value for element</param>
    /// <returns>Modified Skia paint</returns>
    private SKPaint GetSpinePaint(SKPaint paint, float sample)
    {
        var profile = Configuration.Current.ColorProfiles[Configuration.Current.ActiveProfile];
        if (profile.FgColors.Count > 1)
        {
            var pos = (profile.FgColors.Count - 1) * sample;
            var color1 = SKColor.Parse(profile.FgColors[(int)Math.Floor(pos)]);
            var color2 = SKColor.Parse(profile.FgColors[(int)Math.Ceiling(pos)]);
            var weight = sample < 1 ? pos % 1 : 1;
            paint.Color = new SKColor(
                (byte)(color1.Red * (1 - weight) + color2.Red * weight),
                (byte)(color1.Green * (1 - weight) + color2.Green * weight),
                (byte)(color1.Blue * (1 - weight) + color2.Blue * weight),
                (byte)(color1.Alpha * (1 - weight) + color2.Alpha * weight));
        }
        return paint;
    }
}
