using NickvisionCavalier.GNOME.Controls;
using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Events;
using NickvisionCavalier.Shared.Models;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public partial class MainWindow : Adw.ApplicationWindow
{
    private delegate void GAsyncReadyCallback(nint source, nint res, nint user_data);

    [LibraryImport("libEGL.so", StringMarshalling = StringMarshalling.Utf8)]
    private static partial IntPtr eglGetProcAddress(string name);
    [LibraryImport("libGL.so", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void glClear(uint mask);

    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private GRContext? _ctx;
    private SKSurface? _skSurface;
    private float[]? _sample;

    [Gtk.Connect] private readonly Adw.WindowTitle _title;
    [Gtk.Connect] private readonly Gtk.GLArea _glArea;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        SetDefaultSize(800, 600);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        if (_controller.IsDevVersion)
        {
            AddCssClass("devel");
        }
        //Build UI
        builder.Connect(this);
        _title.SetTitle(_controller.AppInfo.ShortName);
        _glArea.OnRealize += (sender, e) =>
        {
            _glArea.MakeCurrent();
            var grInt = GRGlInterface.Create(eglGetProcAddress);
            _ctx = GRContext.CreateGl(grInt);
        };
        _glArea.OnResize += RecreateImageSurfaces;
        _controller.Cava.OutputReceived += (sender, sample) =>
        {
            _sample = sample;
            _glArea.QueueRender();
        };
        _glArea.OnRender += OnRender;
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += Preferences;
        AddAction(actPreferences);
        application.SetAccelsForAction("win.preferences", new string[] { "<Ctrl>comma" });
        //Keyboard Shortcuts Action
        var actKeyboardShortcuts = Gio.SimpleAction.New("keyboardShortcuts", null);
        actKeyboardShortcuts.OnActivate += KeyboardShortcuts;
        AddAction(actKeyboardShortcuts);
        application.SetAccelsForAction("win.keyboardShortcuts", new string[] { "<Ctrl>question" });
        //Quit Action
        var actQuit = Gio.SimpleAction.New("quit", null);
        actQuit.OnActivate += Quit;
        AddAction(actQuit);
        application.SetAccelsForAction("win.quit", new string[] { "<Ctrl>q" });
        //About Action
        var actAbout = Gio.SimpleAction.New("about", null);
        actAbout.OnActivate += About;
        AddAction(actAbout);
        application.SetAccelsForAction("win.about", new string[] { "F1" });
    }

    /// <summary>
    /// Constructs a MainWindow
    /// </summary>
    /// <param name="controller">The MainWindowController</param>
    /// <param name="application">The Adw.Application</param>
    public MainWindow(MainWindowController controller, Adw.Application application) : this(Builder.FromFile("window.ui"), controller, application)
    {
    }

    /// <summary>
    /// (Re)creates image surfaces
    /// </summary>
    /// <param name="sender">Gtk.Widget</param>
    /// <param name="e">EventArgs</param>
    private void RecreateImageSurfaces(Gtk.Widget sender, EventArgs e)
    {
        _skSurface?.Dispose();
        var imgInfo = new SKImageInfo(sender.GetAllocatedWidth(), sender.GetAllocatedHeight());
        _skSurface = SKSurface.Create(_ctx, false, imgInfo);
        _controller.SetCanvas(_skSurface.Canvas);
    }

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

    /// <summary>
    /// Starts the MainWindow
    /// </summary>
    public void Start()
    {
        _application.AddWindow(this);
        Present();
    }

    /// <summary>
    /// Occurs when the preferences action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Preferences(Gio.SimpleAction sender, EventArgs e)
    {
        var preferencesDialog = new PreferencesDialog(_controller.CreatePreferencesViewController(), _application, this);
        preferencesDialog.Present();
    }

    /// <summary>
    /// Occurs when the keyboard shortcuts action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void KeyboardShortcuts(Gio.SimpleAction sender, EventArgs e)
    {
        var builder = Builder.FromFile("shortcuts_dialog.ui");
        var shortcutsWindow = (Gtk.ShortcutsWindow)builder.GetObject("_shortcuts");
        shortcutsWindow.SetTransientFor(this);
        shortcutsWindow.SetIconName(_controller.AppInfo.ID);
        shortcutsWindow.Present();
    }

    /// <summary>
    /// Occurs when quit action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Quit(Gio.SimpleAction sender, EventArgs e) => _application.Quit();

    /// <summary>
    /// Occurs when the about action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void About(Gio.SimpleAction sender, EventArgs e)
    {
        var debugInfo = new StringBuilder();
        debugInfo.AppendLine(_controller.AppInfo.ID);
        debugInfo.AppendLine(_controller.AppInfo.Version);
        debugInfo.AppendLine($"GTK {Gtk.Functions.GetMajorVersion()}.{Gtk.Functions.GetMinorVersion()}.{Gtk.Functions.GetMicroVersion()}");
        debugInfo.AppendLine($"libadwaita {Adw.Functions.GetMajorVersion()}.{Adw.Functions.GetMinorVersion()}.{Adw.Functions.GetMicroVersion()}");
        if (File.Exists("/.flatpak-info"))
        {
            debugInfo.AppendLine("Flatpak");
        }
        else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP")))
        {
            debugInfo.AppendLine("Snap");
        }
        debugInfo.AppendLine(CultureInfo.CurrentCulture.ToString());
        var localeProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "locale",
                UseShellExecute = false,
                RedirectStandardOutput = true
            }
        };
        try
        {
            localeProcess.Start();
            var localeString = localeProcess.StandardOutput.ReadToEnd().Trim();
            localeProcess.WaitForExit();
            debugInfo.AppendLine(localeString);
        }
        catch
        {
            debugInfo.AppendLine("Unknown locale");
        }
        var dialog = Adw.AboutWindow.New();
        dialog.SetTransientFor(this);
        dialog.SetIconName(_controller.AppInfo.ID);
        dialog.SetApplicationName(_controller.AppInfo.ShortName);
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.GetIsDevelVersion() ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetDebugInfo(debugInfo.ToString());
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2023");
        dialog.SetWebsite(_controller.AppInfo.GitHubRepo.ToString());
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.SetDevelopers(string.Format(_("Fyodor Sobolev {0}\nContributors on GitHub ❤️ {1}"), "https://github.com/fsobolev", "https://github.com/fsobolev/CavalierNext/graphs/contributors").Split("\n"));
        dialog.SetDesigners(string.Format(_("Fyodor Sobolev {0}"), "https://github.com/fsobolev").Split("\n"));
        dialog.SetArtists(string.Format(_("Fyodor Sobolev {0}"), "https://github.com/fsobolev").Split("\n"));
        dialog.SetTranslatorCredits(_("translator-credits"));
        dialog.SetReleaseNotes(_controller.AppInfo.Changelog);
        dialog.Present();
    }
}
