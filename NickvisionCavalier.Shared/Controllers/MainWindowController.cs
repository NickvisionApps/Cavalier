using NickvisionCavalier.Shared.Models;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
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

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
    }

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();

    /// <summary>
    /// Saves the MainWindow size to configuration
    /// </summary>
    public void SaveWindowSize(uint width, uint height)
    {
        Configuration.Current.WindowWidth = width;
        Configuration.Current.WindowHeight = height;
        Configuration.Current.Save();
    }
}
