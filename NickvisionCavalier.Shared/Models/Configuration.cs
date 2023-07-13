using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration
{
    public static readonly string ConfigDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}{Path.DirectorySeparatorChar}{AppInfo.Current.Name}";
    private static readonly string ConfigPath = $"{ConfigDir}{Path.DirectorySeparatorChar}config.json";
    private static Configuration? _instance;

    /// <summary>
    /// Main window width
    /// </summary>
    public uint WindowWidth { get; set; }
    /// <summary>
    /// Main window width
    /// </summary>
    public uint WindowHeight { get; set; }
    /// <summary>
    /// Size of drawing area margins in pixels
    /// </summary>
    public uint AreaMargin { get; set; }
    /// <summary>
    /// Whether the window should be borderless
    /// </summary>
    public bool Borderless { get; set; }
    /// <summary>
    /// Whether the corners of the window should be sharp
    /// </summary>
    public bool SharpCorners { get; set; }
    /// <summary>
    /// Whether to show window controls
    /// </summary>
    public bool ShowControls { get; set; }
    /// <summary>
    /// Whether to autohide the headerbar
    /// </summary>
    public bool AutohideHeader { get; set; }
    /// <summary>
    /// CAVA framerate
    /// </summary>
    public uint Framerate { get; set; }
    /// <summary>
    /// Number of bar pairs in CAVA
    /// </summary>
    public uint BarPairs { get; set; }
    /// <summary>
    /// Whether to enable autosens in CAVA
    /// </summary>
    public bool Autosens { get; set; }
    /// <summary>
    /// Manual sensitivity (will be squared when passed to CAVA)
    /// </summary>
    public uint Sensitivity { get; set; }
    /// <summary>
    /// Whether to set channels to stereo (mono if false)
    /// </summary>
    public bool Stereo { get; set; }
    /// <summary>
    /// Whether to enable monstercat smoothing
    /// </summary>
    public bool Monstercat { get; set; }
    /// <summary>
    /// Noise reduction value (0.0-1.0)
    /// </summary>
    public float NoiseReduction { get; set; } // Note: noise reduction will be int (0-100) in the next stable release of CAVA
    /// <summary>
    /// Whether to reverse bars order for each channel
    /// </summary>
    public bool ReverseOrder { get; set; }
    /// <summary>
    /// Drawing direction
    /// </summary>
    public DrawingDirection Direction { get; set; }
    /// <summary>
    /// The size of spaces between elements
    /// </summary>
    public float ItemsOffset { get; set; }
    /// <summary>
    /// Roundness of items (0 - square, 1 - round)
    /// </summary>
    public float ItemsRoundness { get; set; }
    /// <summary>
    /// Whether to fill or draw lines
    /// </summary>
    public bool Filling { get; set; }
    /// <summary>
    /// Thickness of lines when filling is off (in pixels)
    /// </summary>
    public float LinesThickness { get; set; }
    /// <summary>
    /// Active drawing mode
    /// </summary>
    public DrawingMode Mode { get; set; }
    /// <summary>
    /// Mirror mode
    /// </summary>
    public Mirror Mirror { get; set; }
    /// <summary>
    /// Whether to reverse mirrored bars
    /// </summary>
    public bool ReverseMirror { get; set; }
    /// <summary>
    /// List of color profiles
    /// </summary>
    public List<ColorProfile> ColorProfiles { get; set; }
    /// <summary>
    /// Active color profile index
    /// </summary>
    public int ActiveProfile { get; set; }

    /// <summary>
    /// Occurs when the configuration is saved to disk
    /// </summary>
    public event EventHandler? Saved;

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        if (!Directory.Exists(ConfigDir))
        {
            Directory.CreateDirectory(ConfigDir);
        }
        WindowWidth = 400;
        WindowHeight = 200;
        AreaMargin = 0;
        Borderless = false;
        SharpCorners = false;
        ShowControls = false;
        AutohideHeader = false;
        Framerate = 60;
        BarPairs = 6;
        Autosens = true;
        Sensitivity = 10;
        Stereo = true;
        Monstercat = true;
        NoiseReduction = 0.77f;
        ReverseOrder = true;
        Direction = DrawingDirection.BottomTop;
        ItemsOffset = 0.1f;
        ItemsRoundness = 0.5f;
        Filling = true;
        LinesThickness = 15;
        Mode = DrawingMode.WaveBox;
        Mirror = Mirror.Off;
        ReverseMirror = false;
        ColorProfiles = new List<ColorProfile> { new ColorProfile() };
        ActiveProfile = 0;
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current
    {
        get
        {
            if (_instance == null)
            {
                try
                {
                    _instance = JsonSerializer.Deserialize<Configuration>(File.ReadAllText(ConfigPath)) ?? new Configuration();
                    if (_instance.ColorProfiles.Count == 0) // Color profiles list should not be empty
                    {
                        _instance.ColorProfiles = new List<ColorProfile> { new ColorProfile() };
                        _instance.ActiveProfile = 0;
                    }
                }
                catch
                {
                    _instance = new Configuration();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(this));
        Saved?.Invoke(this, EventArgs.Empty);
    }
}
