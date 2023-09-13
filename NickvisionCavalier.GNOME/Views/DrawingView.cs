using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using SkiaSharp;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The DrawingView to render CAVA's output
/// </summary>
public partial class DrawingView : Gtk.Stack, IDisposable
{
    public delegate bool GSourceFunc(nint data);

    [LibraryImport("libEGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint eglGetProcAddress(string name);
    [LibraryImport("libGL.so.1", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void glClear(uint mask);

    [Gtk.Connect] private readonly Adw.StatusPage _welcomeStatus;
    [Gtk.Connect] private readonly Gtk.GLArea _glArea;
    [Gtk.Connect] private readonly Gtk.DrawingArea _cairoArea;

    private bool _disposed;
    private readonly DrawingViewController _controller;
    private readonly bool _useCairo;
    private readonly GSourceFunc _showGl;
    private readonly GSourceFunc _queueRender;
    private readonly Timer _renderTimer;
    private bool _showWelcome;
    private GRContext? _ctx;
    private SKImageInfo? _imgInfo;
    private SKSurface? _glSurface;
    private Cairo.ImageSurface? _cairoSurface;
    private SKSurface? _skSurface;
    private float[]? _sample;

    private DrawingView(Gtk.Builder builder, MainWindow window, DrawingViewController controller) : base(builder.GetPointer("_root"), false)
    {
        _disposed = false;
        _controller = controller;
        _showWelcome = true;
        //Build UI
        builder.Connect(this);
        window.OnNotify += (sender, e) =>
        {
            if ((e.Pspec.GetName() == "default-width" || e.Pspec.GetName() == "default-height") && _showWelcome)
            {
                window.GetDefaultSize(out var width, out var height);
                _welcomeStatus.SetIconName(width < 380 || height < 280 ? "" : "man-dancing");
            }
        };
        if (Environment.GetEnvironmentVariable("CAVALIER_RENDERER")?.ToLower() == "cairo")
        {
            _useCairo = true;
            _cairoArea.OnResize += CreateCairoSurface;
            _cairoArea.SetDrawFunc(CairoDrawFunc);
        }
        else
        {
            _useCairo = false;
            _glArea.OnRealize += (sender, e) =>
            {
                _glArea.MakeCurrent();
                var grInt = GRGlInterface.Create(eglGetProcAddress);
                _ctx = GRContext.CreateGl(grInt);
            };
            _glArea.OnResize += CreateGLSurface;
            _glArea.OnRender += OnRender;
        }
        _controller.CAVA.OutputReceived += (sender, sample) =>
        {
            _sample = sample;
            if (_showWelcome && sample.Any(s => s != 0))
            {
                _showWelcome = false;
            }
            GLib.Functions.IdleAdd(0, () =>
            {
                if (_showWelcome && GetVisibleChildName() != "welcome")
                {
                    SetVisibleChildName("welcome");
                }
                if (!_showWelcome && GetVisibleChildName() != (_useCairo ? "cairo" : "gl"))
                {
                    SetVisibleChildName(_useCairo ? "cairo" : "gl");
                }
                return false;
            });
        };
        _renderTimer = new Timer(1000.0 / _controller.Framerate);
        _renderTimer.Elapsed += (sender, e) =>
        {
            _renderTimer.Interval = 1000.0 / _controller.Framerate;
            GLib.Functions.IdleAdd(0, () =>
            {
                if (_useCairo)
                {
                    _cairoArea.QueueDraw();
                }
                else
                {
                    _glArea.QueueRender();
                }
                return false;
            });
        };
        _renderTimer.Start();
    }

    /// <summary>
    /// Constructs a DrawingView
    /// </summary>
    /// <param name="window">Main window</param>
    /// <param name="controller">The DrawingViewController</param>
    public DrawingView(MainWindow window, DrawingViewController controller) : this(Builder.FromFile("drawing_view.ui"), window, controller)
    {
    }

    /// <summary>
    /// Finalizes the DrawingView
    /// </summary>
    ~DrawingView() => Dispose(false);

    /// <summary>
    /// Frees resources used by the DrawingView object
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the DrawingView object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _controller.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// (Re)creates drawing surface when using OpenGL
    /// </summary>
    /// <param name="sender">GLArea</param>
    /// <param name="e">GLArea.ResizeSignalArgs</param>
    private void CreateGLSurface(Gtk.GLArea sender, Gtk.GLArea.ResizeSignalArgs e)
    {
        _glSurface?.Dispose();
        _skSurface?.Dispose();
        _imgInfo = new SKImageInfo(e.Width, e.Height);
        _glSurface = SKSurface.Create(_ctx, false, _imgInfo.Value);
        if (_glSurface != null)
        {
            _skSurface = SKSurface.Create(_imgInfo.Value);
            _controller.Canvas = _skSurface.Canvas;
        }
    }

    /// <summary>
    /// (Re)creates drawing surface when using Cairo
    /// </summary>
    /// <param name="sender">DrawingArea</param>
    /// <param name="e">DrawingArea.ResizeSignalArgs</param>
    private void CreateCairoSurface(Gtk.DrawingArea sender, Gtk.DrawingArea.ResizeSignalArgs e)
    {
        _skSurface?.Dispose();
        _imgInfo = new SKImageInfo(e.Width, e.Height);
        _cairoSurface = new Cairo.ImageSurface(Cairo.Format.Argb32, _imgInfo.Value.Width, _imgInfo.Value.Height);
        if (_cairoSurface != null)
        {
            _skSurface = SKSurface.Create(_imgInfo.Value, Cairo.Internal.ImageSurface.GetData(_cairoSurface.Handle), _imgInfo.Value.RowBytes);
            _controller.Canvas = _skSurface.Canvas;
        }
    }

    /// <summary>
    /// Occurs on GLArea render frames
    /// </summary>
    /// <param name="sender">GLArea</param>
    /// <param name="e">GLArea.RenderSignalArgs</param>
    /// <returns>Whether or not the event was handled</returns>
    private bool OnRender(Gtk.GLArea sender, Gtk.GLArea.RenderSignalArgs e)
    {
        if (_skSurface == null)
        {
            return false;
        }
        glClear(16384);
        if (_sample != null)
        {
            _controller.Render(_sample, _imgInfo!.Value.Width, _imgInfo.Value.Height);
            _glSurface!.Canvas.Clear();
            using var image = _skSurface.Snapshot();
            _glSurface.Canvas.DrawImage(image, 0, 0);
            _glSurface.Canvas.Flush();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Occurs when DrawingArea should be redrawn
    /// </summary>
    /// <param name="area">DrawingArea</param>
    /// <param name="ctx">Cairo context of the DrawingArea</param>
    /// <param name="width">Area width</param>
    /// <param name="height">Area height</param>
    public void CairoDrawFunc(Gtk.DrawingArea area, Cairo.Context ctx, int width, int height)
    {
        if (_skSurface == null)
        {
            return;
        }
        if (_sample != null)
        {
            _controller.Render(_sample, (float)width, (float)height);
            _cairoSurface!.Flush();
            _cairoSurface.MarkDirty();
            ctx.SetSourceSurface(_cairoSurface, 0, 0);
            ctx.Paint();
        }
    }

    /// <summary>
    /// Occurs when settings for CAVA have changed
    /// </summary>
    /// <param name="sender">object</param>
    /// <param name="e">EventArgs</param>
    public void UpdateCAVASettings(object sender, EventArgs e)
    {
        SetVisibleChildName("load");
        _controller.CAVA.Restart();
    }
}