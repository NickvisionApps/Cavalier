using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
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

    private readonly MainWindowController _controller;
    private readonly Adw.Application _application;
    private readonly DrawingView _drawingView;
    private readonly PreferencesDialog _preferencesDialog;

    private MainWindow(Gtk.Builder builder, MainWindowController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        _application = application;
        //Build UI
        builder.Connect(this);
        SetDefaultSize((int)_controller.WindowWidth, (int)_controller.WindowHeight);
        SetTitle(_controller.AppInfo.ShortName);
        SetIconName(_controller.AppInfo.ID);
        _drawingView = new DrawingView(new DrawingViewController());
        _overlay.SetChild(_drawingView);
        var prefController = _controller.CreatePreferencesViewController();
        prefController.OnWindowSettingsChanged += UpdateWindowSettings;
        prefController.OnCavaSettingsChanged += _drawingView.UpdateCavaSettings;
        _preferencesDialog = new PreferencesDialog(prefController);
        OnCloseRequest += (sender, e) =>
        {
            prefController.Save(); // Save configuration in case preferences dialog is opened
            return false;
        };
        UpdateWindowSettings(null, EventArgs.Empty);
        OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "is-active" && _controller.AutohideHeader)
            {
                _headerRevealer.SetRevealChild(GetIsActive());
            }
        };
        OnCloseRequest += (sender, e) =>
        {
            _controller.SaveWindowSize((uint)DefaultWidth, (uint)DefaultHeight);
            return false;
        };
        //Preferences Action
        var actPreferences = Gio.SimpleAction.New("preferences", null);
        actPreferences.OnActivate += (sender, e) => _preferencesDialog.Present();
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
    private void UpdateWindowSettings(object? sender, EventArgs e)
    {
        _application.StyleManager!.ColorScheme = _controller.Theme switch
        {
            Theme.Light => Adw.ColorScheme.ForceLight,
            _ => Adw.ColorScheme.ForceDark
        };
        _drawingView.SetMarginTop(_controller.Borderless ? 0 : 1);
        _drawingView.SetMarginStart(_controller.Borderless ? 0 : 1);
        _drawingView.SetMarginEnd(_controller.Borderless ? 0 : 1);
        _drawingView.SetMarginBottom(_controller.Borderless ? 0 : 1);
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
        dialog.SetWebsite("https://nickvision.org/");
        dialog.SetIssueUrl(_controller.AppInfo.IssueTracker.ToString());
        dialog.SetSupportUrl(_controller.AppInfo.SupportUrl.ToString());
        dialog.AddLink(_("GitHub Repo"), _controller.AppInfo.GitHubRepo.ToString());
        dialog.AddLink(_("Matrix Chat"), "https://matrix.to/#/#nickvision:matrix.org");
        dialog.SetDevelopers(string.Format(_("Fyodor Sobolev {0}\nNicholas Logozzo {1}\nContributors on GitHub ❤️ {2}"), "https://github.com/fsobolev", "https://github.com/nlogozzo", "https://github.com/NickvisionApps/Cavalier/graphs/contributors").Split("\n"));
        dialog.SetDesigners(string.Format(_("Fyodor Sobolev {0}"), "https://github.com/fsobolev").Split("\n"));
        dialog.SetArtists(string.Format(_("Fyodor Sobolev {0}"), "https://github.com/fsobolev").Split("\n"));
        dialog.SetTranslatorCredits(_("translator-credits"));
        dialog.SetReleaseNotes(_controller.AppInfo.Changelog);
        dialog.Present();
    }
}
