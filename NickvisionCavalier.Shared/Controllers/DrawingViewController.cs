using NickvisionCavalier.Shared.Models;
using SkiaSharp;

namespace NickvisionCavalier.Shared.Controllers;

public class DrawingViewController
{
    private readonly Renderer _renderer;
    
    /// <summary>
    /// Cava instance
    /// </summary>
    public Cava Cava { get; init; }
    
    public DrawingViewController()
    {
        _renderer = new Renderer();
        Cava = new Cava();
        Cava.Start();
    }

    public SKCanvas? Canvas
    {
        get => _renderer.Canvas;

        set => _renderer.Canvas = value;
    }

    public void Render(float[] sample, float width, float height) => _renderer.Draw(sample, width, height);
}