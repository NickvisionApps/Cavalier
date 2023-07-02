using System;
using NickvisionCavalier.Shared.Models;
using SkiaSharp;

namespace NickvisionCavalier.Shared.Controllers;

public class DrawingViewController : IDisposable
{
    private bool _disposed;
    private readonly Renderer _renderer;
    
    /// <summary>
    /// Cava instance
    /// </summary>
    public Cava Cava { get; init; }
    
    public DrawingViewController()
    {
        _disposed = false;
        _renderer = new Renderer();
        Cava = new Cava();
        Cava.Start();
    }

    /// <summary>
    /// Finalizes the DrawingViewController
    /// </summary>
    ~DrawingViewController() => Dispose(false);

    /// <summary>
    /// CAVA framerate
    /// </summary>
    public uint Framerate => Configuration.Current.Framerate;

    /// <summary>
    /// SKCanvas to draw on
    /// </summary>
    public SKCanvas? Canvas
    {
        get => _renderer.Canvas;

        set => _renderer.Canvas = value;
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the Account object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            Cava.Dispose();
        }
        _disposed = true;
    }

    public void Render(float[] sample, float width, float height) => _renderer.Draw(sample, width, height);
}