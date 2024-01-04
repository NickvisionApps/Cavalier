using CommandLine;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// Command-line options
/// </summary>
public class CmdOptions
{
    /// <summary>
    /// Active drawing mode
    /// </summary>
    [Option('d', "drawing-mode", Required = false, HelpText = "Drawing mode, one of: wavebox, levelsbox, particlesbox, barsbox, spinebox, splitterbox, wavecircle, levelscircle, particlescircle, barscircle, spinecircle")]
    public DrawingMode? Mode { get; set; }
    /// <summary>
    /// Mirror mode
    /// </summary>
    [Option('m', "mirror-mode", Required = false, HelpText = "Mirror more, one of: off, full, splitchannels (stereo only)")]
    public Mirror? Mirror { get; set; }
    /// <summary>
    /// Whether to reverse mirrored bars
    /// </summary>
    [Option('v', "reverse-mirror", Required = false, HelpText = "Reverse mirror (true or false)")]
    public bool? ReverseMirror { get; set; }
    /// <summary>
    /// Size of drawing area margins in pixels (0-40)
    /// </summary>
    [Option('n', "margin", Required = false, HelpText = "Area margin (0-40)")]
    public uint? AreaMargin { get; set; }
    /// <summary>
    /// Drawing area X offset in percent of its width (-50-50)
    /// </summary>
    [Option('x', "offset-x", Required = false, HelpText = "Drawing area X offset (-50-50)")]
    public int? AreaOffsetX { get; set; }
    /// <summary>
    /// Drawing area Y offset in percent of its height (-50-50)
    /// </summary>
    [Option('y', "offset-y", Required = false, HelpText = "Drawing area Y offset (-50-50)")]
    public int? AreaOffsetY { get; set; }
    /// <summary>
    /// Drawing direction
    /// </summary>
    [Option('g', "drawing-direction", Required = false, HelpText = "Drawing direction, one of: topbottom, bottomtop, leftright, rightleft")]
    public DrawingDirection? Direction { get; set; }
    /// <summary>
    /// The size of spaces between elements (0-20)
    /// </summary>
    [Option('t', "items-offset", Required = false, HelpText = "Offset between items (0-20)")]
    public uint? ItemsOffset { get; set; }
    /// <summary>
    /// Roundness of items (0 - square, 100 - round)
    /// </summary>
    [Option('r', "items-roundness", Required = false, HelpText = "Roundness of items (0-100)")]
    public uint? ItemsRoundness { get; set; }
    /// <summary>
    /// Whether to fill or draw lines
    /// </summary>
    [Option('f', "filling", Required = false, HelpText = "Fill (true) or draw lines (false)")]
    public bool? Filling { get; set; }
    /// <summary>
    /// Thickness of lines in pixels when filling is off (0-100)
    /// </summary>
    [Option('l', "lines-thickness", Required = false, HelpText = "Lines thickness when filling is off (0-100)")]
    public uint? LinesThickness { get; set; }
    /// <summary>
    /// Whether the window should be borderless
    /// </summary>
    [Option('w', "borderless", Required = false, HelpText = "Make window borderless (true or false)")]
    public bool? Borderless { get; set; }
    /// <summary>
    /// Whether the corners of the window should be sharp
    /// </summary>
    [Option('s', "sharp-corners", Required = false, HelpText = "Make window corneres sharp (true or false)")]
    public bool? SharpCorners { get; set; }
    /// <summary>
    /// Number of bar pairs in CAVA (3-50)
    /// </summary>
    [Option('b', "bar-pairs", Required = false, HelpText = "Number of bar pairs (3-50)")]
    public uint? BarPairs { get; set; }
    /// <summary>
    /// Whether to set channels to stereo (mono if false)
    /// </summary>
    [Option('c', "stereo", Required = false, HelpText = "Mono (false) or stereo (true)")]
    public bool? Stereo { get; set; }
    /// <summary>
    /// Whether to reverse bars order for each channel
    /// </summary>
    [Option('e', "reverse-order", Required = false, HelpText = "Reverse order of bars (true or false)")]
    public bool? ReverseOrder { get; set; }
    /// <summary>
    /// Inner circle radius ratio in circle modes (20-80)
    /// </summary>
    [Option('u', "radius", Required = false, HelpText = "Inner circle radius in circle modes (20-80)")]
    public uint? InnerRadius { get; set; }
    /// <summary>
    /// Rotation in circle modes (0-360)
    /// </summary>
    [Option('o', "rotation", Required = false, HelpText = "Rotation in circle modes (0-360)")]
    public uint? Rotation { get; set; }
    /// <summary>
    /// Active color profile index
    /// </summary>
    [Option('p', "profile", Required = false, HelpText = "Zero-based index of profile to activate")]
    public uint? ActiveProfile { get; set; }
    /// <summary>
    /// Active background color
    /// </summary>
    [Option("bg", Required = false, HelpText = "Background color, hex format: aarrbbgg or rrbbgg")]
    public string? BgColor { get; set; }
    /// <summary>
    /// Index of a background image to load (0 to not load anything)
    /// </summary>
    [Option("bg-index", Required = false, HelpText = "Index of background image to draw (0 for no image)")]
    public int? BgImageIndex { get; set; }
    /// <summary>
    /// Background image scale (10-100, 100 - fill the window)
    /// </summary>
    [Option("bg-scale", Required = false, HelpText = "Background image scale in percent (10-100)")]
    public uint? BgImageScale { get; set; }
    /// <summary>
    /// Background image transparency (10-100, 100 - fully opaque)
    /// </summary>
    [Option("bg-alpha", Required = false, HelpText = "Background image alpha in percent (10-100)")]
    public uint? BgImageAlpha { get; set; }
    /// <summary>
    /// Active forground color
    /// </summary>
    [Option("fg", Required = false, HelpText = "Foreground color, hex format: aarrbbgg or rrbbgg")]
    public string? FgColor { get; set; }
    /// <summary>
    /// Index of a foreground image to load (0 to not load anything)
    /// </summary>
    [Option("fg-index", Required = false, HelpText = "Index of foreground image to draw (0 for no image)")]
    public int? FgImageIndex { get; set; }
    /// <summary>
    /// Foreground image scale (10-100, 100 - fill the window)
    /// </summary>
    [Option("fg-scale", Required = false, HelpText = "Foreground image scale in percent (10-100)")]
    public uint? FgImageScale { get; set; }
    /// <summary>
    /// Foreground image transparency (10-100, 100 - fully opaque)
    /// </summary>
    [Option("fg-alpha", Required = false, HelpText = "Foreground image alpha in percent (10-100)")]
    public uint? FgImageAlpha { get; set; }
    /// <summary>
    /// Whether to replace Spine mode with Hearts mode (easter egg)
    /// </summary>
    /// <remarks>Suggested by my beloved Xenia &lt;3</remarks>
    [Option("hearts", Required = false, HelpText = "Replace Spine mode with Hearts mode")]
    public bool Hearts { get; set; }
}