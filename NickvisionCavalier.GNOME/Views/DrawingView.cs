using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The DrawingView to render CAVA's output
/// </summary>
public partial class DrawingView : Gtk.Overlay
{
    [LibraryImport("libEGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr eglGetProcAddress(string name);
    //TODO: GLX and WGL
    [LibraryImport("libGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void glClear(uint mask);
    
    [Gtk.Connect] private readonly Gtk.GLArea _glArea;

    private readonly DrawingViewController _controller;
    private GRContext? _ctx;
    private SKSurface? _skSurface;
    private float[]? _sample;
    
    private DrawingView(Gtk.Builder builder, DrawingViewController controller) : base(builder.GetPointer("_root"), false)
    {
        _controller = controller;
        //Build UI
        builder.Connect(this);
        _glArea.OnRealize += (sender, e) =>
        {
            _glArea.MakeCurrent();
            var grInt = GRGlInterface.Create(eglGetProcAddress);
            _ctx = GRContext.CreateGl(grInt);
        };
        _glArea.OnResize += OnResize;
        _controller.Cava.OutputReceived += (sender, sample) =>
        {
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
    /// (Re)creates surface on area resize
    /// </summary>
    /// <param name="sender">Gtk.GLArea</param>
    /// <param name="e">EventArgs</param>
    private void OnResize(Gtk.GLArea sender, EventArgs e)
    {
        _skSurface?.Dispose();
        var imgInfo = new SKImageInfo(sender.GetAllocatedWidth(), sender.GetAllocatedHeight());
        _skSurface = SKSurface.Create(_ctx, false, imgInfo);
        _controller.Canvas = _skSurface.Canvas;
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
            return true;
        }
        return false;
    }
}