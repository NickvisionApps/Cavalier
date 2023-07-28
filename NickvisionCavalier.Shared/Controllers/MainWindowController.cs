using Nickvision.Aura;
using NickvisionCavalier.Shared.Models;
using System;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    public Aura Aura { get; init; }
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => Aura.Active.AppInfo;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.ColorProfiles[Configuration.Current.ActiveProfile].Theme;
    /// <summary>
    /// The MainWindow width
    /// </summary>
    public uint WindowWidth => Configuration.Current.WindowWidth;
    /// <summary>
    /// The MainWindow height
    /// </summary>
    public uint WindowHeight => Configuration.Current.WindowHeight;
    /// <summary>
    /// Whether the main window is maximized or not
    /// </summary>
    public bool WindowMaximized => Configuration.Current.WindowMaximized;
    /// <summary>
    /// Whether the window should be borderless
    /// </summary>
    public bool Borderless => Configuration.Current.Borderless;
    /// <summary>
    /// Whether the corners of the window should be sharp
    /// </summary>
    public bool SharpCorners => Configuration.Current.SharpCorners;
    /// <summary>
    /// Whether to show window controls
    /// </summary>
    public bool ShowControls => Configuration.Current.ShowControls;
    /// <summary>
    /// Whether to autohide the headerbar
    /// </summary>
    public bool AutohideHeader => Configuration.Current.AutohideHeader;

    public PreferencesViewController PreferencesViewController { get; init; }

    public event Action? RaiseCommandReceived;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    /// <param name="args">Command-line arguments</param>
    public MainWindowController(string[] args)
    {
        Aura = new Aura("org.nickvision.cavalier", "Nickvision Cavalier", _("Cavalier"), _("Visualize audio with CAVA"));
        Aura.Active.SetConfig<Configuration>("config");
        AppInfo.Version = "2023.8.0-beta1";
        AppInfo.SourceRepo = new Uri("https://github.com/NickvisionApps/Cavalier");
        AppInfo.IssueTracker = new Uri("https://github.com/NickvisionApps/Cavalier/issues/new");
        AppInfo.SupportUrl = new Uri("https://github.com/NickvisionApps/Cavalier/discussions");
        AppInfo.ExtraLinks[_("Matrix Chat")] = new Uri("https://matrix.to/#/#nickvision:matrix.org");
        AppInfo.Developers[_("Fyodor Sobolev")] = new Uri("https://github.com/fsobolev");
        AppInfo.Developers[_("Nicholas Logozzo")] = new Uri("https://github.com/nlogozzo");
        AppInfo.Developers[_("Contributors on GitHub ❤️")] = new Uri("https://github.com/NickvisionApps/Cavalier/graphs/contributors");
        AppInfo.Designers[_("Fyodor Sobolev")] = new Uri("https://github.com/fsobolev");
        AppInfo.Artists[_("David Lapshin")] = new Uri("https://github.com/daudix-UFO");
        AppInfo.TranslatorCredits = _("translator-credits");
        PreferencesViewController = new PreferencesViewController();
        var ipc = Aura.Communicate(args);
        ipc.CommandReceived += HandleCommandLine;
        HandleCommandLine(this, args);
    }

    /// <summary>
    /// Process command-line arguments passed when starting the app or from other instances
    /// </summary>
    /// <param name="sender">Sender or event, either this or IPCServer</param>
    /// <param name="args">Command-line arguments</param>
    public void HandleCommandLine(object? sender, string[] args)
    {
        if (args.Length == 0)
        {
            RaiseCommandReceived?.Invoke();
        }
        else
        {
            PreferencesViewController.HandleCommandLine(sender, args);
        }
    }

    /// <summary>
    /// Saves the MainWindow size to configuration
    /// </summary>
    /// <param name="width">Window width</param>
    /// <param name="height">Windiw height</param>
    /// <param name="maximized">Whether the window is maximized or not</param>
    public void SaveWindowSize(uint width, uint height, bool maximized)
    {
        Configuration.Current.WindowWidth = width;
        Configuration.Current.WindowHeight = height;
        Configuration.Current.WindowMaximized = maximized;
        Aura.Active.SaveConfig("config");
    }
}
