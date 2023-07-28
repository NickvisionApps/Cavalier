using Nickvision.Aura;
using System.Collections.Generic;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration : IConfiguration
{
    /// <summary>
    /// Main window width
    /// </summary>
    public uint WindowWidth { get; set; }
    /// <summary>
    /// Main window height
    /// </summary>
    public uint WindowHeight { get; set; }
    /// <summary>
    /// Whether main window is maximized or not
    /// </summary>
    public bool WindowMaximized { get; set; }
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
    /// Index of a background image to load (-1 to not load anything)
    /// </summary>
    public int ImageIndex { get; set; }
    /// <summary>
    /// Background image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float ImageScale { get; set; }

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
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
        LinesThickness = 5;
        Mode = DrawingMode.WaveBox;
        Mirror = Mirror.Off;
        ReverseMirror = false;
        ColorProfiles = new List<ColorProfile> { new ColorProfile() };
        ActiveProfile = 0;
        ImageIndex = -1;
        ImageScale = 1.0f;
    }

    /// <summary>
    /// Gets the singleton object
    /// </summary>
    internal static Configuration Current
    {
        get
        {
            var obj = (Configuration)Aura.Active.ConfigFiles["config"];
            // Ensure that we have at least 1 color profile
            if (obj.ColorProfiles.Count == 0)
            {
                obj.ColorProfiles = new List<ColorProfile> { new ColorProfile() };
                obj.ActiveProfile = 0;
            }
            return obj;
        }
    }
}
