using NickvisionCavalier.Shared.Models;
using System;

namespace NickvisionCavalier.Shared.Controllers;

/// <summary>
/// A controller for a PreferencesView
/// </summary>
public class PreferencesViewController
{
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;

    /// <summary>
    /// Constructs a PreferencesViewController
    /// </summary>
    internal PreferencesViewController()
    {

    }

    /// <summary>
    /// Size of drawing area margins in pixels
    /// </summary>
    public uint AreaMargin
    {
        get => Configuration.Current.AreaMargin;

        set => Configuration.Current.AreaMargin = value;
    }

    /// <summary>
    /// Whether the window should be borderless
    /// </summary>
    public bool Borderless
    {
        get => Configuration.Current.Borderless;

        set => Configuration.Current.Borderless = value;
    }

    /// <summary>
    /// Whether the corners of the window should be sharp
    /// </summary>
    public bool SharpCorners
    {
        get => Configuration.Current.SharpCorners;

        set => Configuration.Current.SharpCorners = value;
    }

    /// <summary>
    /// Whether to show window controls
    /// </summary>
    public bool ShowControls
    {
        get => Configuration.Current.ShowControls;

        set => Configuration.Current.ShowControls = value;
    }

    /// <summary>
    /// Whether to autohide the headerbar
    /// </summary>
    public bool AutohideHeader
    {
        get => Configuration.Current.AutohideHeader;

        set => Configuration.Current.AutohideHeader = value;
    }

    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme
    {
        get => Configuration.Current.Theme;

        set => Configuration.Current.Theme = value;
    }

    public event Action? OnWindowSettingsChanged;

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    private void SaveConfiguration() => Configuration.Current.Save();

    /// <summary>
    /// Occurs when a window's setting has changed
    /// </summary>
    public void ChangeWindowSettings()
    {
        OnWindowSettingsChanged?.Invoke();
        SaveConfiguration();
    }
}
