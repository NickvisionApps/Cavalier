using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// A model for the configuration of the application
/// </summary>
public class Configuration : ConfigurationBase
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
    /// Drawing area X offset (-0.5-0.5)
    /// </summary>
    public float AreaOffsetX { get; set; }
    /// <summary>
    /// Drawing area Y offset (-0.5-0.5)
    /// </summary>
    public float AreaOffsetY { get; set; }
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
    /// Noise reduction value (0.15-0.95)
    /// </summary>
    public float NoiseReduction { get; set; }
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
    /// Inner circle radius ratio in circle modes (0.2-0.8)
    /// </summary>
    public float InnerRadius { get; set; }
    /// <summary>
    /// Rotation angle in radians in circle modes (0-2PI)
    /// </summary>
    public float Rotation { get; set; }
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
    public int BgImageIndex { get; set; }
    /// <summary>
    /// Background image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float BgImageScale { get; set; }
    /// <summary>
    /// Background image transparency (0.1-1.0, 1.0 - fully opaque)
    /// </summary>
    public float BgImageAlpha { get; set; }
    /// <summary>
    /// Index of a foreground image to load (-1 to not load anything)
    /// </summary>
    public int FgImageIndex { get; set; }
    /// <summary>
    /// Foreground image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float FgImageScale { get; set; }
    /// <summary>
    /// Foreground image transparency (0.1-1.0, 1.0 - fully opaque)
    /// </summary>
    public float FgImageAlpha { get; set; }
    /// <summary>
    /// Whether to replace Spine mode with Hearts mode (easter egg)
    /// </summary>
    /// <remarks>Suggested by my beloved Xenia &lt;3</remarks>
    [JsonIgnore]
    public bool Hearts { get; set; }

    /// <summary>
    /// Constructs a Configuration
    /// </summary>
    public Configuration()
    {
        WindowWidth = 500;
        WindowHeight = 300;
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
        InnerRadius = 0.5f;
        Rotation = 0f;
        ColorProfiles = new List<ColorProfile> { new ColorProfile() };
        ActiveProfile = 0;
        BgImageIndex = -1;
        BgImageScale = 1f;
        BgImageAlpha = 1f;
        FgImageIndex = -1;
        FgImageScale = 1f;
        FgImageAlpha = 1f;
        Hearts = false;
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
