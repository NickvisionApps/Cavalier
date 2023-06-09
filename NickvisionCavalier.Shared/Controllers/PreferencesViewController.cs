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

    public event EventHandler? OnWindowSettingsChanged;
    public event EventHandler? OnCavaSettingsChanged;

    /// <summary>
    /// Constructs a PreferencesViewController
    /// </summary>
    internal PreferencesViewController()
    {

    }

    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme
    {
        get => Configuration.Current.Theme;

        set => Configuration.Current.Theme = value;
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
    /// CAVA framerate
    /// </summary>
    public uint Framerate
    {
        get => Configuration.Current.Framerate;

        set => Configuration.Current.Framerate = value;
    }

    /// <summary>
    /// Number of bar pairs in CAVA
    /// </summary>
    public uint BarPairs
    {
        get => Configuration.Current.BarPairs;

        set => Configuration.Current.BarPairs = value;
    }

    /// <summary>
    /// Whether to enable autosens in CAVA
    /// </summary>
    public bool Autosens
    {
        get => Configuration.Current.Autosens;

        set => Configuration.Current.Autosens = value;
    }

    /// <summary>
    /// Manual sensitivity (will be squared when passed to CAVA)
    /// </summary>
    public uint Sensitivity
    {
        get => Configuration.Current.Sensitivity;

        set => Configuration.Current.Sensitivity = value;
    }

    /// <summary>
    /// Whether to set channels to stereo (mono if false)
    /// </summary>
    public bool Stereo
    {
        get => Configuration.Current.Stereo;

        set => Configuration.Current.Stereo = value;
    }

    /// <summary>
    /// Whether to enable monstercat smoothing
    /// </summary>
    public bool Monstercat
    {
        get => Configuration.Current.Monstercat;

        set => Configuration.Current.Monstercat = value;
    }

    /// <summary>
    /// Noise reduction value (0.0-1.0)
    /// </summary>
    public float NoiseReduction
    {
        get => Configuration.Current.NoiseReduction;

        set => Configuration.Current.NoiseReduction = value;
    }

    /// <summary>
    /// Whether to reverse bars order for each channel
    /// </summary>
    public bool ReverseOrder
    {
        get => Configuration.Current.ReverseOrder;

        set => Configuration.Current.ReverseOrder = value;
    }

    /// <summary>
    /// Drawing direction
    /// </summary>
    public DrawingDirection Direction
    {
        get => Configuration.Current.Direction;

        set => Configuration.Current.Direction = value;
    }

    /// <summary>
    /// The size of spaces between elements
    /// </summary>
    public float ItemsOffset
    {
        get => Configuration.Current.ItemsOffset;

        set => Configuration.Current.ItemsOffset = value;
    }

    /// <summary>
    /// Roundness of items (0 - square, 1 - round)
    /// </summary>
    public float ItemsRoundness
    {
        get => Configuration.Current.ItemsRoundness;

        set => Configuration.Current.ItemsRoundness = value;
    }

    /// <summary>
    /// Whether to fill or draw lines
    /// </summary>
    public bool Filling
    {
        get => Configuration.Current.Filling;

        set => Configuration.Current.Filling = value;
    }

    /// <summary>
    /// Thickness of lines when filling is off (in pixels)
    /// </summary>
    public uint LinesThickness
    {
        get => Configuration.Current.LinesThickness;

        set => Configuration.Current.LinesThickness = value;
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void Save() => Configuration.Current.Save();

    /// <summary>
    /// Occurs when a window's setting has changed
    /// </summary>
    public void ChangeWindowSettings()
    {
        OnWindowSettingsChanged?.Invoke(this, EventArgs.Empty);
        Save();
    }

    /// <summary>
    /// Occurs when a CAVA setting has changed
    /// </summary>
    public void ChangeCavaSettings()
    {
        OnCavaSettingsChanged?.Invoke(this, EventArgs.Empty);
        Save();
    }
}
