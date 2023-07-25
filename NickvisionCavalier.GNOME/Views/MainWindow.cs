using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Timers;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The MainWindow for the application
/// </summary>
public class MainWindow : Adw.ApplicationWindow
{
    [Gtk.Connect] private readonly Gtk.Overlay _overlay;
    [Gtk.Connect] private readonly Gtk.Revealer _headerRevealer;
    [Gtk.Connect] private readonly Adw.HeaderBar _header;
    [Gtk.Connect] private readonly Adw.Bin _resizeBin;

    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private readonly DrawingView _drawingView;
    private readonly PreferencesViewController _preferencesController;
    private readonly Timer _resizeTimer;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        //Build UI
        builder.Connect(this);
        _controller.RaiseCommandReceived += Present;
        SetDefaultSize((int)_controller.WindowWidth, (int)_controller.WindowHeight);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        _drawingView = new DrawingView(new DrawingViewController());
        _overlay.SetChild(_drawingView);
        _preferencesController = _controller.GetPreferencesViewController();
        _preferencesController.OnWindowSettingsChanged += UpdateWindowSettings;
        _preferencesController.OnCAVASettingsChanged += _drawingView.UpdateCAVASettings;
        var preferencesDialog = new PreferencesDialog(_preferencesController, application);
        OnCloseRequest += OnClose;
        UpdateWindowSettings();
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "is-active" && _controller.AutohideHeader)
            {
                _headerRevealer.SetRevealChild(GetIsActive());
            }
        };
        _resizeTimer = new Timer(400);
        _resizeTimer.AutoReset = false;
        _resizeTimer.Elapsed += (sender, e) => _resizeBin.SetVisible(false);
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "default-width" || e.Pspec.GetName() == "default-height")
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
                _resizeBin.SetVisible(true);
            }
            else if (e.Pspec.GetName() == "maximized" || e.Pspec.GetName() == "fullscreened")
            {
                SetDrawingAreaMargins();
            }
        };
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += (sender, e) => preferencesDialog.Present();
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
        //Fullscreen Action
        var actFullscreen = Gio.SimpleAction.New("fullscreen", null);
        actFullscreen.OnActivate += (sender, e) =>
        {
            if(Fullscreened)
            {
                Unfullscreen();
            }
            else
            {
                Fullscreen();
            }
        };
        AddAction(actFullscreen);
        application.SetAccelsForAction("win.fullscreen", new string[] { "F11" });
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
    /// Starts the MainWindow
    /// </summary>
    public void Start()
    {
        _application.AddWindow(this);
        Present();
    }

    /// <summary>
    /// Occurs when settings for the window have changed
    /// </summary>
    private void UpdateWindowSettings()
    {
        _application.StyleManager!.ColorScheme = _controller.Theme switch
        {
            Theme.Light => Adw.ColorScheme.ForceLight,
            _ => Adw.ColorScheme.ForceDark
        };
        SetDrawingAreaMargins();
        if (_controller.Borderless)
        {
            AddCssClass("borderless-window");
        }
        else
        {
            RemoveCssClass("borderless-window");
        }
        if (_controller.SharpCorners)
        {
            AddCssClass("sharp-corners");
        }
        else
        {
            RemoveCssClass("sharp-corners");
        }
        _header.SetShowStartTitleButtons(_controller.ShowControls);
        _header.SetShowEndTitleButtons(_controller.ShowControls);
        _headerRevealer.SetRevealChild(GetIsActive() || !_controller.AutohideHeader);
    }

    /// <summary>
    /// Sets correct margins for drawing area
    /// </summary>
    private void SetDrawingAreaMargins()
    {
        _drawingView.SetMarginTop(_controller.Borderless || IsMaximized() || Fullscreened ? 0 : 1);
        _drawingView.SetMarginStart(_controller.Borderless || IsMaximized() || Fullscreened ? 0 : 1);
        _drawingView.SetMarginEnd(_controller.Borderless || IsMaximized() || Fullscreened ? 0 : 1);
        _drawingView.SetMarginBottom(_controller.Borderless || IsMaximized() || Fullscreened ? 0 : 1);
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
        shortcutsWindow.SetIconName(_controller.AppInfo.ID);
        shortcutsWindow.Present();
    }

    /// <summary>
    /// Occurs when closing the window
    /// </summary>
    /// <param name="sender">Gtk.Window</param>
    /// <param name="e">EventArgs</param>
    private bool OnClose(Gtk.Window sender, EventArgs e)
    {
        _controller.SaveWindowSize((uint)DefaultWidth, (uint)DefaultHeight);
        _preferencesController.Save(); // Save configuration in case preferences dialog is opened
        _drawingView.Dispose();
        return false;
    }

    /// <summary>
    /// Occurs when quit action is triggered
    /// </summary>
    /// <param name="sender">Gio.SimpleAction</param>
    /// <param name="e">EventArgs</param>
    private void Quit(Gio.SimpleAction sender, EventArgs e)
    {
        OnClose(this, EventArgs.Empty);
        _application.Quit();
    }

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
        dialog.SetApplicationIcon(_controller.AppInfo.ID + (_controller.AppInfo.IsDevVersion ? "-devel" : ""));
        dialog.SetVersion(_controller.AppInfo.Version);
        dialog.SetDebugInfo(debugInfo.ToString());
        dialog.SetComments(_controller.AppInfo.Description);
        dialog.SetDeveloperName("Nickvision");
        dialog.SetLicenseType(Gtk.License.MitX11);
        dialog.SetCopyright("© Nickvision 2023");
        dialog.SetWebsite("https://nickvision.org/");
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_("GitHub Repo"), _controller.AppInfo.SourceRepo.ToString());
        foreach (var pair in _controller.AppInfo.ExtraLinks)
        {
            dialog.AddLink(pair.Key, pair.Value.ToString());
        }
        dialog.SetDevelopers(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Developers));
        dialog.SetDesigners(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Designers));
        dialog.SetArtists(_controller.AppInfo.ConvertURLDictToArray(_controller.AppInfo.Artists));
        dialog.SetTranslatorCredits(_controller.AppInfo.TranslatorCredits);
        dialog.SetReleaseNotes(_controller.AppInfo.HTMLChangelog);
        dialog.Present();
    }
}
