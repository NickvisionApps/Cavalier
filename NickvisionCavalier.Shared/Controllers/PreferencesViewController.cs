using NickvisionCavalier.Shared.Models;
using System;
using System.Collections.Generic;

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
    public float LinesThickness
    {
        get => Configuration.Current.LinesThickness;

        set => Configuration.Current.LinesThickness = value;
    }

    /// <summary>
    /// Active drawing mode
    /// </summary>
    public DrawingMode Mode
    {
        get => Configuration.Current.Mode;

        set => Configuration.Current.Mode = value;
    }

    /// <summary>
    /// Mirror mode
    /// </summary>
    public Mirror Mirror
    {
        get => Configuration.Current.Mirror;

        set => Configuration.Current.Mirror = value;
    }

    /// <summary>
    /// Whether to reverse mirrored bars
    /// </summary>
    public bool ReverseMirror
    {
        get => Configuration.Current.ReverseMirror;

        set => Configuration.Current.ReverseMirror = value;
    }

    /// <summary>
    /// List of color profiles
    /// </summary>
    public List<ColorProfile> ColorProfiles => Configuration.Current.ColorProfiles;

    /// <summary>
    /// Active color profile index
    /// </summary>
    public int ActiveProfile
    {
        get => Configuration.Current.ActiveProfile;

        set => Configuration.Current.ActiveProfile = value;
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
    }

    /// <summary>
    /// Occurs when a CAVA setting has changed
    /// </summary>
    public void ChangeCavaSettings()
    {
        OnCavaSettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Adds new color profile
    /// </summary>
    /// <param name="name">New profile name</param>
    public void AddColorProfile(string name)
    {
        var newProfile = (ColorProfile)ColorProfiles[ActiveProfile].Clone();
        newProfile.Name = name;
        ColorProfiles.Add(newProfile);
        ActiveProfile = ColorProfiles.Count - 1;
    }

    /// <summary>
    /// Add color to active profile
    /// </summary>
    /// <param name="type">Color type</param>
    /// <param name="color">Color string (#aarrggbb)</param>
    public void AddColor(ColorType type, string color)
    {
        if (type == ColorType.Foreground)
        {
            ColorProfiles[ActiveProfile].FgColors.Add(color);
        }
        else
        {
            ColorProfiles[ActiveProfile].BgColors.Add(color);
        }
    }

    /// <summary>
    /// Edit color in active profile
    /// </summary>
    /// <param name="type">Color type</param>
    /// <param name="index">Color index</param>
    /// <param name="color">Color string (#aarrggbb)</param>
    public void EditColor(ColorType type, int index, string color)
    {
        if (type == ColorType.Foreground)
        {
            ColorProfiles[ActiveProfile].FgColors[index] = color;
        }
        else
        {
            ColorProfiles[ActiveProfile].BgColors[index] = color;
        }
    }

    /// <summary>
    /// Deletes a color from active profile
    /// </summary>
    /// <param name="type">Color type</param>
    /// <param name="index">Color index</param>
    public void DeleteColor(ColorType type, int index)
    {
        if (type == ColorType.Foreground)
        {
            ColorProfiles[ActiveProfile].FgColors.RemoveAt(index);
        }
        else
        {
            ColorProfiles[ActiveProfile].BgColors.RemoveAt(index);
        }
    }
}
