using Nickvision.Aura;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// An object that renders the picture
/// </summary>
public class Renderer
{
    private enum GradientType
    {
        Background = 0,
        Foreground
    }

    private delegate SKPath? DrawFunc(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint);
    private float _oldWidth;
    private float _oldHeight;
    private int _bgImageIndex;
    private SKBitmap? _bgImageBitmap;
    private SKBitmap? _bgTargetBitmap;
    private float _oldBgScale;
    private int _fgImageIndex;
    private SKBitmap? _fgImageBitmap;
    private SKBitmap? _fgTargetBitmap;
    private float _oldFgScale;

    /// <summary>
    /// Renderer's canvas to draw on
    /// </summary>
    public SKCanvas? Canvas { get; set; }

    /// <summary>
    /// Construct Renderer
    /// </summary>
    public Renderer()
    {
        Canvas = null;
        _bgImageIndex = -1;
        _fgImageIndex = -1;
        _oldWidth = 0;
        _oldHeight = 0;
        _oldBgScale = 0f;
        _oldFgScale = 0f;
    }

    /// <summary>
    /// Draw picture
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="width">Picture width</param>
    /// <param name="height">Picture height</param>
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
            bgPaint.Shader = CreateGradient(GradientType.Background, profile.BgColors, width, height);
        }
        else
        {
            bgPaint.Color = SKColor.Parse(profile.BgColors[0]);
        }
        Canvas.DrawRect(0, 0, width, height, bgPaint);
        // Prepare images
        if (_bgImageIndex != Configuration.Current.BgImageIndex)
        {
            _bgImageBitmap?.Dispose();
            _bgTargetBitmap?.Dispose();
            if (Configuration.Current.BgImageIndex != -1)
            {
                var images = new List<string>();
                foreach (var file in Directory.GetFiles($"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}images"))
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        images.Add(file);
                    }
                }
                images.Sort();
                if (Configuration.Current.BgImageIndex < images.Count)
                {
                    _bgImageBitmap = SKBitmap.Decode(images[Configuration.Current.BgImageIndex]);
                    _oldBgScale = 0f; // To enforce redraw
                }
                else
                {
                    Configuration.Current.BgImageIndex = -1;
                }
            }
            _bgImageIndex = Configuration.Current.BgImageIndex;
        }
        if (_fgImageIndex != Configuration.Current.FgImageIndex)
        {
            _fgImageBitmap?.Dispose();
            _fgTargetBitmap?.Dispose();
            if (Configuration.Current.FgImageIndex != -1)
            {
                var images = new List<string>();
                foreach (var file in Directory.GetFiles($"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}images"))
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                    {
                        images.Add(file);
                    }
                }
                images.Sort();
                if (Configuration.Current.FgImageIndex < images.Count)
                {
                    _fgImageBitmap = SKBitmap.Decode(images[Configuration.Current.FgImageIndex]);
                    _oldFgScale = 0f; // To enforce redraw
                }
                else
                {
                    Configuration.Current.FgImageIndex = -1;
                }
            }
            _fgImageIndex = Configuration.Current.FgImageIndex;
        }
        if (_oldWidth != width || _oldHeight != height || Math.Abs(_oldBgScale - Configuration.Current.BgImageScale) > 0.01f)
        {
            _oldBgScale = Configuration.Current.BgImageScale;
            if (_bgImageIndex != -1)
            {
                var bgScale = Math.Max(width / _bgImageBitmap!.Width, height / _bgImageBitmap.Height);
                var bgRect = new SKRect(0, 0, _bgImageBitmap.Width * bgScale, _bgImageBitmap.Height * bgScale);
                _bgTargetBitmap?.Dispose();
                _bgTargetBitmap = new SKBitmap((int)(bgRect.Width * Configuration.Current.BgImageScale), (int)(bgRect.Height * Configuration.Current.BgImageScale));
                _bgImageBitmap.ScalePixels(_bgTargetBitmap, SKFilterQuality.Medium);
            }
        }
        if (_oldWidth != width || _oldHeight != height || Math.Abs(_oldFgScale - Configuration.Current.FgImageScale) > 0.01f)
        {
            _oldFgScale = Configuration.Current.FgImageScale;
            if (_fgImageIndex != -1)
            {
                var fgScale = Math.Max(width / _fgImageBitmap!.Width, height / _fgImageBitmap.Height);
                var fgRect = new SKRect(0, 0, _fgImageBitmap.Width * fgScale, _fgImageBitmap.Height * fgScale);
                _fgTargetBitmap?.Dispose();
                _fgTargetBitmap = new SKBitmap((int)(fgRect.Width * Configuration.Current.FgImageScale), (int)(fgRect.Height * Configuration.Current.FgImageScale));
                _fgImageBitmap.ScalePixels(_fgTargetBitmap, SKFilterQuality.Medium);
            }
        }
        _oldWidth = width;
        _oldHeight = height;
        // Draw background image
        if (_bgImageIndex != -1)
        {
            using var paint = new SKPaint();
            paint.Color = paint.Color.WithAlpha((byte)(255 * Configuration.Current.BgImageAlpha));
            Canvas.DrawBitmap(_bgTargetBitmap, width / 2 - _bgTargetBitmap!.Width / 2f, height / 2 - _bgTargetBitmap.Height / 2f, paint);
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
        if (profile.FgColors.Count > 1 && Configuration.Current.Mode != DrawingMode.SpineBox && Configuration.Current.Mode != DrawingMode.SpineCircle)
        {
            fgPaint.Shader = CreateGradient(GradientType.Foreground, profile.FgColors, width, height, Configuration.Current.AreaMargin);
        }
        else
        {
            fgPaint.Color = SKColor.Parse(profile.FgColors[0]);
        }
        DrawFunc drawFunc = Configuration.Current.Mode switch
        {
            DrawingMode.LevelsBox => DrawLevelsBox,
            DrawingMode.ParticlesBox => DrawParticlesBox,
            DrawingMode.BarsBox => DrawBarsBox,
            DrawingMode.SpineBox => DrawSpineBox,
            DrawingMode.SplitterBox => DrawSplitterBox,
            DrawingMode.WaveCircle => DrawWaveCircle,
            DrawingMode.LevelsCircle => DrawLevelsCircle,
            DrawingMode.ParticlesCircle => DrawParticlesCircle,
            DrawingMode.BarsCircle => DrawBarsCircle,
            DrawingMode.SpineCircle => DrawSpineCircle,
            _ => DrawWaveBox,
        };
        if (Configuration.Current.Mirror == Mirror.Full)
        {
            using var path1 = drawFunc(sample, Configuration.Current.Direction,
                (width + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetX + Configuration.Current.AreaMargin,
                (height + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetY + Configuration.Current.AreaMargin,
                GetMirrorWidth(width), GetMirrorHeight(height),
                Configuration.Current.Rotation, fgPaint);
            using var path2 = drawFunc(Configuration.Current.ReverseMirror ? sample.Reverse().ToArray() : sample, GetMirrorDirection(),
                (width + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetX + GetMirrorX(width),
                (height + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetY + GetMirrorY(height),
                GetMirrorWidth(width), GetMirrorHeight(height),
                -Configuration.Current.Rotation, fgPaint);
            if (path1 != null && path2 != null && _fgImageIndex != -1)
            {
                using var path = new SKPath();
                path.AddPath(path1);
                path.AddPath(path2);
                Canvas.Save();
                Canvas.ClipPath(path);
                using var paint = new SKPaint();
                paint.Color = paint.Color.WithAlpha((byte)(255 * Configuration.Current.FgImageAlpha));
                Canvas.DrawBitmap(_fgTargetBitmap,
                    (width + Configuration.Current.AreaMargin * 2) / 2f - _fgTargetBitmap!.Width / 2f,
                    (height + Configuration.Current.AreaMargin * 2) / 2f - _fgTargetBitmap.Height / 2f, paint);
                Canvas.Restore();
            }
        }
        else if (Configuration.Current.Mirror == Mirror.SplitChannels)
        {
            using var path1 = drawFunc(sample.Take(sample.Length / 2).ToArray(), Configuration.Current.Direction,
                (width + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetX + Configuration.Current.AreaMargin,
                (height + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetY + Configuration.Current.AreaMargin,
                GetMirrorWidth(width), GetMirrorHeight(height),
                Configuration.Current.Rotation, fgPaint);
            using var path2 = drawFunc(Configuration.Current.ReverseMirror ? sample.Skip(sample.Length / 2).ToArray() : sample.Skip(sample.Length / 2).Reverse().ToArray(), GetMirrorDirection(),
                (width + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetX + GetMirrorX(width),
                (height + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetY + GetMirrorY(height),
                GetMirrorWidth(width), GetMirrorHeight(height),
                -Configuration.Current.Rotation, fgPaint);
            if (path1 != null && path2 != null && _fgImageIndex != -1)
            {
                using var path = new SKPath();
                path.AddPath(path1);
                path.AddPath(path2);
                Canvas.Save();
                Canvas.ClipPath(path);
                using var paint = new SKPaint();
                paint.Color = paint.Color.WithAlpha((byte)(255 * Configuration.Current.FgImageAlpha));
                Canvas.DrawBitmap(_fgTargetBitmap, width / 2 - _fgTargetBitmap!.Width / 2f, height / 2 - _fgTargetBitmap.Height / 2f, paint);
                Canvas.Restore();
            }
        }
        else
        {
            using var path = drawFunc(sample, Configuration.Current.Direction,
                (width + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetX + Configuration.Current.AreaMargin,
                (height + Configuration.Current.AreaMargin * 2) * Configuration.Current.AreaOffsetY + Configuration.Current.AreaMargin,
                width, height,
                Configuration.Current.Rotation, fgPaint);
            if (path != null && _fgImageIndex != -1)
            {
                Canvas.Save();
                Canvas.ClipPath(path);
                using var paint = new SKPaint();
                paint.Color = paint.Color.WithAlpha((byte)(255 * Configuration.Current.FgImageAlpha));
                Canvas.DrawBitmap(_fgTargetBitmap, width / 2 - _fgTargetBitmap!.Width / 2f, height / 2 - _fgTargetBitmap.Height / 2f, paint);
                Canvas.Restore();
            }
        }
        Canvas.Flush();
    }

    /// <summary>
    /// Create gradient shader
    /// </summary>
    /// <param name="type">Gradient type (foreground or background)</param>
    /// <param name="colorStrings">List of colors as strings</param>
    /// <param name="width">Canvas width</param>
    /// <param name="height">Canvas height</param>
    /// <param name="margin">Area margin</param>
    /// <returns>Skia Shader</returns>
    private SKShader CreateGradient(GradientType type, List<string> colorStrings, float width, float height, uint margin = 0)
    {
        var colors = colorStrings.Select(c => SKColor.Parse(c)).Reverse().ToArray();
        if (type == GradientType.Foreground)
        {
            if (Configuration.Current.Mode == DrawingMode.WaveCircle)
            {
                if (Configuration.Current.Mirror > Mirror.Off)
                {
                    width = GetMirrorWidth(width);
                    height = GetMirrorHeight(height);
                }
                var fullRadius = Math.Min(width, height) / 2;
                var innerRadius = fullRadius * Configuration.Current.InnerRadius;
                var positions = new float[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                {
                    positions[i] = (i + (colors.Length - 1 - i) * innerRadius / fullRadius) / (colors.Length - 1);
                }
                return SKShader.CreateRadialGradient(new SKPoint(width / 2, height / 2),
                    fullRadius, colors, positions, SKShaderTileMode.Clamp);
            }
            if (Configuration.Current.Mode > DrawingMode.WaveCircle)
            {
                if (Configuration.Current.Mirror > Mirror.Off)
                {
                    width = GetMirrorWidth(width);
                    height = GetMirrorHeight(height);
                }
                return SKShader.CreateLinearGradient(new SKPoint(margin, Math.Min(width, height) * Configuration.Current.InnerRadius / 2), new SKPoint(margin, Math.Min(width, height) / 2), colors, SKShaderTileMode.Clamp);
            }
        }
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
            DrawingDirection.TopBottom => SKShader.CreateLinearGradient(new SKPoint(margin, margin + height * Configuration.Current.AreaOffsetY), new SKPoint(margin, height * (1 + Configuration.Current.AreaOffsetY)), colors, SKShaderTileMode.Clamp),
            DrawingDirection.BottomTop => SKShader.CreateLinearGradient(new SKPoint(margin, height * (1 + Configuration.Current.AreaOffsetY)), new SKPoint(margin, margin + height * Configuration.Current.AreaOffsetY), colors, SKShaderTileMode.Clamp),
            DrawingDirection.LeftRight => SKShader.CreateLinearGradient(new SKPoint(margin + width * Configuration.Current.AreaOffsetX, margin), new SKPoint(width * (1 + Configuration.Current.AreaOffsetX), margin), colors, SKShaderTileMode.Clamp),
            _ => SKShader.CreateLinearGradient(new SKPoint(width * (1 + Configuration.Current.AreaOffsetX), margin), new SKPoint(margin + width * Configuration.Current.AreaOffsetX, margin), colors, SKShaderTileMode.Clamp)
        };
    }

    /// <summary>
    /// Get direction for mirrored part of picture
    /// </summary>
    /// <returns>DrawingDirection</returns>
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

    /// <summary>
    /// Get X coordinate for mirror top-left corner
    /// </summary>
    /// <returns>X coordinate float</returns>
    private float GetMirrorX(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2f + Configuration.Current.AreaMargin;
        }
        return Configuration.Current.AreaMargin;
    }

    /// <summary>
    /// Get Y coordinate for mirror top-left corner
    /// </summary>
    /// <returns>Y coordinate float</returns>
    private float GetMirrorY(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2f + Configuration.Current.AreaMargin;
        }
        return Configuration.Current.AreaMargin;
    }

    /// <summary>
    /// Get width for mirrored part of picture
    /// </summary>
    /// <returns>Width float</returns>
    private float GetMirrorWidth(float width)
    {
        if (Configuration.Current.Direction == DrawingDirection.LeftRight || Configuration.Current.Direction == DrawingDirection.RightLeft)
        {
            return width / 2f;
        }
        return width;
    }

    /// <summary>
    /// Get height for mirrored part of picture
    /// </summary>
    /// <returns>Height float</returns>
    private float GetMirrorHeight(float height)
    {
        if (Configuration.Current.Direction == DrawingDirection.TopBottom || Configuration.Current.Direction == DrawingDirection.BottomTop)
        {
            return height / 2f;
        }
        return height;
    }

    /// <summary>
    /// Flips a coordinate value to the other side of the screen and ensure it doesn't exceed the maximum value
    /// </summary>
    /// <param name="enabled">Flip coordinate enabled</param>
    /// <param name="screenDimension">Dimension of screen in axis to be flipped</param>
    /// <param name="coordinate">Coordinate in axis to be flipped</param>
    /// <returns>New coordinate in axis</returns>
    private float FlipCoord(bool enabled, float screenDimension, float coordinate)
    {
        var max = Math.Max(0, Math.Min(coordinate, screenDimension));
        return enabled ? screenDimension - max : max;
        // Note: By camping these values, it ensures the resulting bezier curve when smoothed
        // never goes below 0 however, it does mean that it is not as smooth, as some values 
        // may be smoothed to be negative based on the expected gradient
    }

    /// <summary>
    /// Draw picture, Wave mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="paint">Skia paint</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawWaveBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / (sample.Length - 1);
        var path = new SKPath();
        var flipImage = false;
        var pointsArray = new (float x, float y)[sample.Length];
        var gradientsList = new float[sample.Length];
        switch (direction)
        {
            case DrawingDirection.TopBottom or DrawingDirection.BottomTop:
                flipImage = direction == DrawingDirection.TopBottom;
                // Create a list of point of where the the curve must pass through
                for (var i = 0; i < sample.Length; i++)
                {
                    pointsArray[i] = (step * i, height * (1 - sample[i]));
                }
                // Calculate gradient between the two neighbouring points for every point
                for (var i = 0; i < pointsArray.Length; i++)
                {
                    // Determine the previous and next point
                    // If there isn't one, use the current point
                    var previousPoint = pointsArray[Math.Max(i - 1, 0)];
                    var nextPoint = pointsArray[Math.Min(i + 1, pointsArray.Length - 1)];
                    var gradient = nextPoint.y - previousPoint.y;
                    // If using the current point (when at the edges)
                    // then the run in rise/run = 1, otherwise a two step run exists
                    gradientsList[i] = i == 0 || i == pointsArray.Length - 1 ? gradient : gradient / 2;
                }
                var yOffset = y + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
                path.MoveTo(x + pointsArray[0].x, yOffset + FlipCoord(flipImage, height, pointsArray[0].y));
                for (var i = 0; i < pointsArray.Length - 1; i++)
                {
                    path.CubicTo(
                        x + pointsArray[i].x + step * 0.5f,
                        yOffset + FlipCoord(flipImage, height, pointsArray[i].y + gradientsList[i] * 0.5f),
                        x + pointsArray[i + 1].x + step * -0.5f,
                        yOffset + FlipCoord(flipImage, height, pointsArray[i + 1].y + gradientsList[i + 1] * -0.5f),
                        x + pointsArray[i + 1].x,
                        yOffset + FlipCoord(flipImage, height, pointsArray[i + 1].y));
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + width, y + FlipCoord(flipImage, height, height));
                    path.LineTo(x, y + FlipCoord(flipImage, height, height));
                    path.Close();
                }
                break;
            case DrawingDirection.LeftRight or DrawingDirection.RightLeft:
                flipImage = direction == DrawingDirection.RightLeft;
                for (var i = 0; i < sample.Length; i++)
                {
                    pointsArray[i] = (width * sample[i], step * i);
                }
                for (var i = 0; i < pointsArray.Length; i++)
                {
                    var previousPoint = pointsArray[Math.Max(i - 1, 0)];
                    var nextPoint = pointsArray[Math.Min(i + 1, pointsArray.Length - 1)];
                    var gradient = nextPoint.x - previousPoint.x;
                    gradientsList[i] = i == 0 || i == pointsArray.Length - 1 ? gradient : gradient / 2;
                }
                var xOffset = x - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
                path.MoveTo(xOffset + FlipCoord(flipImage, width, pointsArray[0].x), y + pointsArray[0].y);
                for (var i = 0; i < pointsArray.Length - 1; i++)
                {
                    path.CubicTo(
                        xOffset + FlipCoord(flipImage, width, pointsArray[i].x + gradientsList[i] * 0.5f),
                        y + pointsArray[i].y + step * 0.5f,
                        xOffset + FlipCoord(flipImage, width, pointsArray[i + 1].x + gradientsList[i + 1] * -0.5f),
                        y + pointsArray[i + 1].y + step * -0.5f,
                        xOffset + FlipCoord(flipImage, width, pointsArray[i + 1].x),
                        y + pointsArray[i + 1].y);
                }
                if (Configuration.Current.Filling)
                {
                    path.LineTo(x + FlipCoord(flipImage, width, 0), y + height);
                    path.LineTo(x + FlipCoord(flipImage, width, 0), y);
                    path.Close();
                }
                break;
        }
        Canvas.DrawPath(path, paint);
        if (!Configuration.Current.Filling)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    path.LineTo(x + width, y);
                    path.LineTo(x, y);
                    path.Close();
                    break;
                case DrawingDirection.BottomTop:
                    path.LineTo(x + width, y + height);
                    path.LineTo(x, y + height);
                    path.Close();
                    break;
                case DrawingDirection.LeftRight:
                    path.LineTo(x, y + height);
                    path.LineTo(x, y);
                    path.Close();
                    break;
                case DrawingDirection.RightLeft:
                    path.LineTo(x + width, y + height);
                    path.LineTo(x + width, y);
                    path.Close();
                    break;
            }
        }
        return path;
    }

    /// <summary>
    /// Draw picture, Levels mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawLevelsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 10) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 10 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var path = new SKPath();
        for (var i = 0; i < sample.Length; i++)
        {
            for (var j = 0; j < Math.Floor(sample[i] * 10); j++)
            {
                switch (direction)
                {
                    case DrawingDirection.TopBottom:
                        path.AddRoundRect(
                            new SKRect(
                                x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                y + height / 10 * j + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                x + step * (i + Configuration.Current.ItemsOffset) + itemWidth,
                                y + height / 10 * j + height / 10 * Configuration.Current.ItemsOffset + itemHeight),
                            itemWidth / 2 * Configuration.Current.ItemsRoundness,
                            itemHeight / 2 * Configuration.Current.ItemsRoundness);
                        break;
                    case DrawingDirection.BottomTop:
                        path.AddRoundRect(
                            new SKRect(
                                x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                y + height / 10 * (9 - j) + height / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                x + step * (i + Configuration.Current.ItemsOffset) + itemWidth,
                                y + height / 10 * (9 - j) + height / 10 * Configuration.Current.ItemsOffset + itemHeight),
                            itemWidth / 2 * Configuration.Current.ItemsRoundness,
                            itemHeight / 2 * Configuration.Current.ItemsRoundness);
                        break;
                    case DrawingDirection.LeftRight:
                        path.AddRoundRect(
                            new SKRect(
                                x + width / 10 * j + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                x + width / 10 * j + width / 10 * Configuration.Current.ItemsOffset + itemWidth,
                                y + step * (i + Configuration.Current.ItemsOffset) + itemHeight),
                            itemWidth / 2 * Configuration.Current.ItemsRoundness,
                            itemHeight / 2 * Configuration.Current.ItemsRoundness);
                        break;
                    case DrawingDirection.RightLeft:
                        path.AddRoundRect(
                            new SKRect(
                                x + width / 10 * (9 - j) + width / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                                x + width / 10 * (9 - j) + width / 10 * Configuration.Current.ItemsOffset + itemWidth,
                                y + step * (i + Configuration.Current.ItemsOffset) + itemHeight),
                            itemWidth / 2 * Configuration.Current.ItemsRoundness,
                            itemHeight / 2 * Configuration.Current.ItemsRoundness);
                        break;
                }
            }
        }
        Canvas.DrawPath(path, paint);
        return path;
    }

    /// <summary>
    /// Draw picture, Particles mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawParticlesBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemWidth = (direction < DrawingDirection.LeftRight ? step : width / 11) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var itemHeight = (direction < DrawingDirection.LeftRight ? height / 11 : step) * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2);
        var path = new SKPath();
        for (var i = 0; i < sample.Length; i++)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    path.AddRoundRect(
                        new SKRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 11 * 10 * sample[i] + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            x + step * (i + Configuration.Current.ItemsOffset) + itemWidth,
                            y + height / 11 * 10 * sample[i] + height / 11 * Configuration.Current.ItemsOffset + itemHeight),
                        itemWidth / 2 * Configuration.Current.ItemsRoundness,
                        itemHeight / 2 * Configuration.Current.ItemsRoundness);
                    break;
                case DrawingDirection.BottomTop:
                    path.AddRoundRect(
                        new SKRect(
                            x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + height / 11 * 10 * (1 - sample[i]) + height / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            x + step * (i + Configuration.Current.ItemsOffset) + itemWidth,
                            y + height / 11 * 10 * (1 - sample[i]) + height / 11 * Configuration.Current.ItemsOffset + itemHeight),
                        itemWidth / 2 * Configuration.Current.ItemsRoundness,
                        itemHeight / 2 * Configuration.Current.ItemsRoundness);
                    break;
                case DrawingDirection.LeftRight:
                    path.AddRoundRect(
                        new SKRect(
                            x + width / 11 * 10 * sample[i] + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            x + width / 11 * 10 * sample[i] + width / 11 * Configuration.Current.ItemsOffset + itemWidth,
                            y + step * (i + Configuration.Current.ItemsOffset) + itemHeight),
                        itemWidth / 2 * Configuration.Current.ItemsRoundness,
                        itemHeight / 2 * Configuration.Current.ItemsRoundness);
                    break;
                case DrawingDirection.RightLeft:
                    path.AddRoundRect(
                        new SKRect(
                            x + width / 11 * 10 * (1 - sample[i]) + width / 11 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                            x + width / 11 * 10 * (1 - sample[i]) + width / 11 * Configuration.Current.ItemsOffset + itemWidth,
                            y + step * (i + Configuration.Current.ItemsOffset) + itemHeight),
                        itemWidth / 2 * Configuration.Current.ItemsRoundness,
                        itemHeight / 2 * Configuration.Current.ItemsRoundness);
                    break;
            }
        }
        Canvas.DrawPath(path, paint);
        return path;
    }

    /// <summary>
    /// Draw picture, Bars mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawBarsBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var path = new SKPath();
        for (var i = 0; i < sample.Length; i++)
        {
            if (sample[i] == 0)
            {
                continue;
            }
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                    path.AddRect(new SKRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        (Configuration.Current.Filling ? y : y + Configuration.Current.LinesThickness / 2) - 1,
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2) + step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        y + height * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) + 1));
                    break;
                case DrawingDirection.BottomTop:
                    path.AddRect(new SKRect(
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + height * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2) + step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        y + height - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) + 1));
                    break;
                case DrawingDirection.LeftRight:
                    path.AddRect(new SKRect(
                        Configuration.Current.Filling ? x : x + Configuration.Current.LinesThickness / 2,
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + width * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2) + step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness)));
                    break;
                case DrawingDirection.RightLeft:
                    path.AddRect(new SKRect(
                        x + width * (1 - sample[i]) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        x + width - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                        y + step * (i + Configuration.Current.ItemsOffset) + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2) + step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness)));
                    break;
            };
        }
        Canvas.DrawPath(path, paint);
        return path;
    }

    /// <summary>
    /// Draw picture, Spine mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawSpineBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var itemSize = step * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness);
        var totalPath = new SKPath();
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
                    if (Configuration.Current.Hearts)
                    {
                        Canvas.Save();
                        using var path = new SKPath();
                        CreateHeart(path, itemSize);
                        Canvas.Translate(x + step * i + step / 2, y + height / 2);
                        Canvas.Scale(sample[i]);
                        Canvas.DrawPath(path, GetSpinePaint(paint, sample[i]));
                        Canvas.Restore();
                        break;
                    }
                    Canvas.DrawRoundRect(
                        x + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        y + height / 2 - itemSize * sample[i] / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        GetSpinePaint(paint, sample[i]));
                    totalPath.AddRect(new SKRect(
                        x + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        y + height / 2 - itemSize * sample[i] / 2,
                        x + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2 + itemSize * sample[i],
                        y + height / 2 - itemSize * sample[i] / 2 + itemSize * sample[i]));
                    break;
                case DrawingDirection.LeftRight:
                case DrawingDirection.RightLeft:
                    if (Configuration.Current.Hearts)
                    {
                        Canvas.Save();
                        using var path = new SKPath();
                        CreateHeart(path, itemSize);
                        Canvas.Translate(x + width / 2, y + step * i + step / 2);
                        Canvas.Scale(sample[i]);
                        Canvas.DrawPath(path, GetSpinePaint(paint, sample[i]));
                        Canvas.Restore();
                        break;
                    }
                    Canvas.DrawRoundRect(
                        x + width / 2 - itemSize * sample[i] / 2,
                        y + step * (i + 0.5f) + (1 - itemSize * sample[i]) / 2,
                        itemSize * sample[i], itemSize * sample[i],
                        itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness, itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                        GetSpinePaint(paint, sample[i]));
                    break;
            }
        }
        if (Configuration.Current.Hearts)
        {
            totalPath.Dispose();
            return null;
        }
        return totalPath;
    }

    /// <summary>
    /// Modify path to create a heart for modified Spine mode
    /// </summary>
    /// <param name="path">Path to use for drawing</param>
    /// <param name="itemSize">Size of a square to fit a heart into</param>
    private void CreateHeart(SKPath path, float itemSize)
    {
        path.MoveTo(0, itemSize / 2);
        path.CubicTo(
            0, itemSize / 2.2f,
            -itemSize / 1.8f, itemSize / 3,
            -itemSize / 2, -itemSize / 6);
        path.CubicTo(
            -itemSize / 2.5f, -itemSize / 2,
            -itemSize / 6.5f, -itemSize / 2,
            0, -itemSize / 5.5f);
        path.CubicTo(
            itemSize / 6.5f, -itemSize / 2,
            itemSize / 2.5f, -itemSize / 2,
            itemSize / 2, -itemSize / 6);
        path.CubicTo(
            itemSize / 1.8f, itemSize / 3,
            0, itemSize / 2.2f,
            0, itemSize / 2);
        path.Close();
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
            var pos = (profile.FgColors.Count - 1) * (1 - sample);
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

    /// <summary>
    /// Draw picture, Splitter mode, Box variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawSplitterBox(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var step = (direction < DrawingDirection.LeftRight ? width : height) / sample.Length;
        var path = new SKPath();
        var orient = 1;
        switch (direction)
        {
            case DrawingDirection.TopBottom:
                path.MoveTo(x, y + height / 2 * (1 + sample[0]));
                break;
            case DrawingDirection.BottomTop:
                orient = -1;
                path.MoveTo(x, y + height / 2 * (1 + sample[0] * orient));
                break;
            case DrawingDirection.LeftRight:
                path.MoveTo(x + width / 2 * (1 + sample[0]), y);
                break;
            case DrawingDirection.RightLeft:
                orient = -1;
                path.MoveTo(x + width / 2 * (1 + sample[0] * orient), y);
                break;
        }
        for (var i = 0; i < sample.Length; i++)
        {
            switch (direction)
            {
                case DrawingDirection.TopBottom:
                case DrawingDirection.BottomTop:
                    if (i > 0)
                    {
                        path.LineTo(x + step * i, y + height / 2);
                    }
                    path.LineTo(x + step * i, y + height / 2 * (1 + sample[i] * (i % 2 == 0 ? orient : -orient)));
                    path.LineTo(x + step * (i + 1), y + height / 2 * (1 + sample[i] * (i % 2 == 0 ? orient : -orient)));
                    if (i < sample.Length - 1)
                    {
                        path.LineTo(x + step * (i + 1), y + height / 2);
                    }
                    break;
                case DrawingDirection.LeftRight:
                case DrawingDirection.RightLeft:
                    if (i > 0)
                    {
                        path.LineTo(x + width / 2, y + step * i);
                    }
                    path.LineTo(x + width / 2 * (1 + sample[i] * (i % 2 == 0 ? orient : -orient)), y + step * i);
                    path.LineTo(x + width / 2 * (1 + sample[i] * (i % 2 == 0 ? orient : -orient)), y + step * (i + 1));
                    if (i < sample.Length - 1)
                    {
                        path.LineTo(x + width / 2, y + step * (i + 1));
                    }
                    break;
            }
        }
        if (!Configuration.Current.Filling)
        {
            Canvas.DrawPath(path, paint);
        }
        switch (direction)
        {
            case DrawingDirection.TopBottom:
                path.LineTo(x + width, y);
                path.LineTo(x, y);
                break;
            case DrawingDirection.BottomTop:
                path.LineTo(x + width, y + height);
                path.LineTo(x, y + height);
                break;
            case DrawingDirection.LeftRight:
                path.LineTo(x, y + height);
                path.LineTo(x, y);
                break;
            case DrawingDirection.RightLeft:
                path.LineTo(x + width, y + height);
                path.LineTo(x + width, y);
                break;
        }
        path.Close();
        if (Configuration.Current.Filling)
        {
            Canvas.DrawPath(path, paint);
        }
        return path;
    }

    /// <summary>
    /// Draw picture, Wave mode, Circle variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawWaveCircle(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var fullRadius = Math.Min(width, height) / 2;
        var innerRadius = fullRadius * Configuration.Current.InnerRadius;
        var radius = fullRadius - innerRadius;
        Canvas.Save();
        Canvas.Translate(x, y);
        paint.Style = SKPaintStyle.Stroke;
        paint.StrokeWidth = Configuration.Current.Filling ? fullRadius - innerRadius : Configuration.Current.LinesThickness;
        using var path = new SKPath();
        path.MoveTo(
            width / 2 + (innerRadius + radius * sample[0]) * (float)Math.Cos(Math.PI / 2 + rotation),
            height / 2 + (innerRadius + radius * sample[0]) * (float)Math.Sin(Math.PI / 2 + rotation));
        for (var i = 0; i < sample.Length - 1; i++)
        {
            path.CubicTo(
                width / 2 + (innerRadius + radius * sample[i]) * (float)Math.Cos(Math.PI / 2 + Math.PI * 2 * (i + 0.5f) / sample.Length + rotation),
                height / 2 + (innerRadius + radius * sample[i]) * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * (i + 0.5f) / sample.Length + rotation),
                width / 2 + (innerRadius + radius * sample[i + 1]) * (float)Math.Cos(Math.PI / 2 + Math.PI * 2 * (i + 0.5f) / sample.Length + rotation),
                height / 2 + (innerRadius + radius * sample[i + 1]) * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * (i + 0.5f) / sample.Length + rotation),
                width / 2 + (innerRadius + radius * sample[i + 1]) * (float)Math.Cos(Math.PI / 2 + Math.PI * 2 * (i + 1) / sample.Length + rotation),
                height / 2 + (innerRadius + radius * sample[i + 1]) * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * (i + 1) / sample.Length + rotation));
        }
        path.CubicTo(
            width / 2 + (innerRadius + radius * sample[^1]) * (float)Math.Cos(Math.PI / 2 + Math.PI * 2 * (sample.Length - 0.5f) / sample.Length + rotation),
            height / 2 + (innerRadius + radius * sample[^1]) * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * (sample.Length - 0.5f) / sample.Length + rotation),
            width / 2 + (innerRadius + radius * sample[0]) * (float)Math.Cos(Math.PI / 2 + Math.PI * 2 * (sample.Length - 0.5f) / sample.Length + rotation),
            height / 2 + (innerRadius + radius * sample[0]) * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * (sample.Length - 0.5f) / sample.Length + rotation),
            width / 2 + (innerRadius + radius * sample[0]) * (float)Math.Cos(Math.PI / 2 + rotation),
            height / 2 + (innerRadius + radius * sample[0]) * (float)Math.Sin(Math.PI / 2 + rotation));
        path.Close();
        if (Configuration.Current.Filling)
        {
            Canvas.ClipPath(path, SKClipOperation.Intersect, true);
            Canvas.DrawCircle(new SKPoint(width / 2, height / 2), innerRadius + (fullRadius - innerRadius) / 2, paint);
        }
        else
        {
            Canvas.DrawPath(path, paint);
        }
        Canvas.Restore();
        return null;
    }

    /// <summary>
    /// Draw picture, Levels mode, Circle variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawLevelsCircle(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var fullRadius = Math.Min(width, height) / 2;
        var innerRadius = fullRadius * Configuration.Current.InnerRadius;
        var barWidth = (float)(2 * Math.PI * innerRadius / sample.Length);
        for (var i = 0; i < sample.Length; i++)
        {
            Canvas.Save();
            Canvas.Translate(x + width / 2, y + height / 2);
            Canvas.RotateRadians(2 * (float)Math.PI * (i + 0.5f) / sample.Length + rotation);
            for (var j = 0; j < Math.Floor(sample[i] * 10); j++)
            {
                Canvas.DrawRoundRect(
                    -barWidth * (1 - Configuration.Current.ItemsOffset * 2) / 2 + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                    innerRadius + (fullRadius - innerRadius) / 10 * j + (fullRadius - innerRadius) / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                    barWidth * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                    (fullRadius - innerRadius) / 10 * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                    (barWidth * (1 - Configuration.Current.ItemsOffset) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness)) * Configuration.Current.ItemsRoundness,
                    (fullRadius - innerRadius) / 10 * (1 - Configuration.Current.ItemsOffset) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) * Configuration.Current.ItemsRoundness,
                    paint);
            }
            Canvas.Restore();
        }
        return null;
    }

    /// <summary>
    /// Draw picture, Particles mode, Circle variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawParticlesCircle(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var fullRadius = Math.Min(width, height) / 2;
        var innerRadius = fullRadius * Configuration.Current.InnerRadius;
        var barWidth = (float)(2 * Math.PI * innerRadius / sample.Length);
        for (var i = 0; i < sample.Length; i++)
        {
            Canvas.Save();
            Canvas.Translate(x + width / 2, y + height / 2);
            Canvas.RotateRadians(2 * (float)Math.PI * (i + 0.5f) / sample.Length + rotation);
            Canvas.DrawRoundRect(
                -barWidth * (1 - Configuration.Current.ItemsOffset * 2) / 2 + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                innerRadius + (fullRadius - innerRadius) / 10 * 9 * sample[i] + (fullRadius - innerRadius) / 10 * Configuration.Current.ItemsOffset + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                barWidth * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                (fullRadius - innerRadius) / 10 * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                (barWidth * (1 - Configuration.Current.ItemsOffset) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness)) * Configuration.Current.ItemsRoundness,
                (fullRadius - innerRadius) / 10 * (1 - Configuration.Current.ItemsOffset) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) * Configuration.Current.ItemsRoundness,
                paint);
            Canvas.Restore();
        }
        return null;
    }

    /// <summary>
    /// Draw picture, Bars mode, Circle variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawBarsCircle(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var fullRadius = Math.Min(width, height) / 2;
        var innerRadius = fullRadius * Configuration.Current.InnerRadius;
        var barWidth = (float)(2 * Math.PI * innerRadius / sample.Length);
        for (var i = 0; i < sample.Length; i++)
        {
            Canvas.Save();
            Canvas.Translate(x + width / 2, y + height / 2);
            Canvas.RotateRadians(2 * (float)Math.PI * (i + 0.5f) / sample.Length + rotation);
            Canvas.DrawRect(
                -barWidth * (1 - Configuration.Current.ItemsOffset * 2) / 2 + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                innerRadius + (Configuration.Current.Filling ? 0 : 0 + Configuration.Current.LinesThickness / 2),
                barWidth * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness),
                (fullRadius - innerRadius) * sample[i] - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness) + 1,
                paint);
            Canvas.Restore();
        }
        return null;
    }

    /// <summary>
    /// Draw picture, Spine mode, Circle variant
    /// </summary>
    /// <param name="sample">CAVA sample</param>
    /// <param name="direction">DrawingDirection</param>
    /// <param name="x">Top-left corner X coordinate</param>
    /// <param name="y">Top-left corner Y coordinate</param>
    /// <param name="width">Drawing width</param>
    /// <param name="height">Drawing height</param>
    /// <param name="rotation">Rotation angle in radians (only used in Circle modes)</param>
    /// <param name="paint">Skia paint</param>
    /// <returns>SKPath for foreground image masking if supported, else null</returns>
    private SKPath? DrawSpineCircle(float[] sample, DrawingDirection direction, float x, float y, float width, float height, float rotation, SKPaint paint)
    {
        var fullRadius = Math.Min(width, height) / 2;
        var innerRadius = fullRadius * Configuration.Current.InnerRadius;
        var barWidth = (float)(2 * Math.PI * innerRadius / sample.Length);
        for (var i = 0; i < sample.Length; i++)
        {
            var itemSize = barWidth * (1 - Configuration.Current.ItemsOffset * 2) - (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness);
            if (Configuration.Current.Hearts)
            {
                Canvas.Save();
                using var path = new SKPath();
                CreateHeart(path, itemSize);
                Canvas.Translate(x + width / 2 + innerRadius * (float)Math.Cos(rotation + Math.PI / 2 + Math.PI * 2 * i / sample.Length), y + height / 2 + innerRadius * (float)Math.Sin(Math.PI / 2 + Math.PI * 2 * i / sample.Length));
                Canvas.Scale(sample[i]);
                Canvas.DrawPath(path, GetSpinePaint(paint, sample[i]));
                Canvas.Restore();
                continue;
            }
            Canvas.Save();
            Canvas.Translate(x + width / 2, y + height / 2);
            Canvas.RotateRadians(2 * (float)Math.PI * (i + 0.5f) / sample.Length + rotation);
            Canvas.DrawRoundRect(
                -barWidth * (1 - Configuration.Current.ItemsOffset * 2) / 2 * sample[i] + (Configuration.Current.Filling ? 0 : Configuration.Current.LinesThickness / 2),
                innerRadius - itemSize * sample[i] / 2,
                itemSize * sample[i], itemSize * sample[i],
                itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                itemSize * sample[i] / 2 * Configuration.Current.ItemsRoundness,
                GetSpinePaint(paint, sample[i]));
            Canvas.Restore();
        }
        return null;
    }
}
