using NickvisionCavalier.Shared.Models;
using SkiaSharp;

namespace NickvisionCavalier.Shared.Controllers;

public class DrawingViewController
{
    /// <summary>
    /// Renderer object for DrawingView
    /// <summary>
    private readonly Renderer _renderer;
    
    /// <summary>
    /// Cava instance
    /// </summary>
    public readonly Cava Cava;
    
    public DrawingViewController()
    {
        _renderer = new Renderer();
        Cava = new Cava();
        Cava.Start();
    }

    public void SetCanvas(SKCanvas canvas)
    {
        _renderer.Canvas = canvas;
    }

    public void Render(float[] sample, float width, float height) => _renderer.Draw(sample, width, height);
}