using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The DrawingView to render CAVA's output
/// </summary>
public partial class DrawingView : Gtk.Stack
{
    [LibraryImport("libEGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint eglGetProcAddress(string name);
    [LibraryImport("libGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void glClear(uint mask);

    [Gtk.Connect] private readonly Gtk.GLArea _glArea;

    private readonly DrawingViewController _controller;
    private GRContext? _ctx;
    private SKSurface? _skSurface;
    private float[]? _sample;
    private readonly System.Timers.Timer _antiFreezeTimer;
    
    public event Action? OnFreeze;

    private DrawingView(Gtk.Builder builder, DrawingViewController controller) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _antiFreezeTimer = new System.Timers.Timer(50);
        _antiFreezeTimer.AutoReset = false;
        _antiFreezeTimer.Elapsed += (sender, e) =>
        {
            // GLArea can randomly freeze, stopping to react on QueueRender()
            OnFreeze?.Invoke();
        };
        //Build UI
        builder.Connect(this);
        _glArea.OnRealize += (sender, e) =>
        {
            _glArea.MakeCurrent();
            var grInt = GRGlInterface.Create(eglGetProcAddress);
            _ctx = GRContext.CreateGl(grInt);
        };
        _glArea.OnResize += (sender, e) => CreateSurface();
        _controller.Cava.OutputReceived += (sender, sample) =>
        {
            if (GetVisibleChildName() != "gl")
            {
                SetVisibleChildName("gl");
            }
            _antiFreezeTimer.Start();
            _sample = sample;
            _glArea.QueueRender();
        };
        _glArea.OnRender += OnRender;
    }
    
    /// <summary>
    /// Constructs a DrawingView
    /// </summary>
    /// <param name="controller">The DrawingViewController</param>
    public DrawingView(DrawingViewController controller) : this(Builder.FromFile("drawing_view.ui"), controller)
    {
    }

    /// <summary>
    /// (Re)creates drawing surface
    /// </summary>
    private void CreateSurface()
    {
        _skSurface?.Dispose();
        var imgInfo = new SKImageInfo(_glArea.GetAllocatedWidth(), _glArea.GetAllocatedHeight());
        _skSurface = SKSurface.Create(_ctx, false, imgInfo);
        if (_skSurface != null)
        {
            _controller.Canvas = _skSurface.Canvas;
        }
    }

    /// <summary>
    /// Occurs on GLArea render frames
    /// </summary>
    private bool OnRender(Gtk.GLArea sender, EventArgs e)
    {
        _antiFreezeTimer.Stop();
        if (_skSurface == null)
        {
            return false;
        }
        glClear(16384);
        if (_sample != null)
        {
            _controller.Render(_sample, (float)sender.GetAllocatedWidth(), (float)sender.GetAllocatedHeight());
            return true;
        }
        return false;
    }

    /// <summary>
    /// Occurs when settings for CAVA have changed
    /// </summary>
    public void UpdateCavaSettings(object? sender, EventArgs e)
    {
        SetVisibleChildName("load");
        _controller.Cava.Restart();
    }
}