using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;
using System.Timers;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The DrawingView to render CAVA's output
/// </summary>
public partial class DrawingView : Gtk.Stack
{
    public delegate bool GSourceFunc(nint data);

    [LibraryImport("libEGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint eglGetProcAddress(string name);
    [LibraryImport("libGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void glClear(uint mask);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void g_main_context_invoke(nint context, GSourceFunc function, nint data);

    [Gtk.Connect] private readonly Gtk.GLArea _glArea;

    private readonly DrawingViewController _controller;
    private readonly GSourceFunc _showGl;
    private readonly GSourceFunc _queueRender;
    private readonly Timer _renderTimer;
    private GRContext? _ctx;
    private SKSurface? _glSurface;
    private SKSurface? _skSurface;
    private float[]? _sample;

    private DrawingView(Gtk.Builder builder, DrawingViewController controller) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        _showGl = (x) =>
        {
            if (GetVisibleChildName() != "gl")
            {
                SetVisibleChildName("gl");
            }
            return false;
        };
        _queueRender = (x) =>
        {
            _glArea.QueueRender();
            return false;
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
            _sample = sample;
            g_main_context_invoke(0, _showGl, 0);
        };
        _glArea.OnRender += OnRender;
        _renderTimer = new Timer(1000.0 / _controller.Framerate);
        _renderTimer.Elapsed += (sender, e) =>
        {
            _renderTimer.Interval = 1000.0 / _controller.Framerate;
            g_main_context_invoke(0, _queueRender, 0);
        };
        _renderTimer.Start();
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
        _glSurface?.Dispose();
        _skSurface?.Dispose();
        var imgInfo = new SKImageInfo(_glArea.GetAllocatedWidth(), _glArea.GetAllocatedHeight());
        _glSurface = SKSurface.Create(_ctx, false, imgInfo);
        if (_glSurface != null)
        {
            _skSurface = SKSurface.Create(imgInfo);
            _controller.Canvas = _skSurface.Canvas;
        }
    }

    /// <summary>
    /// Occurs on GLArea render frames
    /// </summary>
    private bool OnRender(Gtk.GLArea sender, EventArgs e)
    {
        if (_skSurface == null)
        {
            return false;
        }
        glClear(16384);
        if (_sample != null)
        {
            _controller.Render(_sample, (float)sender.GetAllocatedWidth(), (float)sender.GetAllocatedHeight());
            _glSurface.Canvas.Clear();
            using var image = _skSurface.Snapshot();
            _glSurface.Canvas.DrawImage(image, 0, 0);
            _glSurface.Canvas.Flush();
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