using CommandLine;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// Command-line options
/// </summary>
public class CmdOptions {
    /// <summary>
    /// Active drawing mode
    /// </summary>
    [Option('d', "drawing-mode", Required = false, HelpText = "Drawing mode, one of: wavebox, levelsbox, particlesbox, barsbox, spinebox, splitterbox")]
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
    /// Size of drawing area margins in pixels
    /// </summary>
    [Option('n', "margin", Required = false, HelpText = "Area margin (0-40)")]
    public uint? AreaMargin { get; set; }
    /// <summary>
    /// Drawing direction
    /// </summary>
    [Option('o', "drawing-direction", Required = false, HelpText = "Drawing direction, one of: topbottom, bottomtop, leftright, rightleft")]
    public DrawingDirection? Direction { get; set; }
    /// <summary>
    /// The size of spaces between elements
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
    /// Thickness of lines when filling is off (in pixels)
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
    /// Number of bar pairs in CAVA
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
    /// Active color profile index
    /// </summary>
    [Option('p', "profile", Required = false, HelpText = "Zero-based index of profile to activate")]
    public uint? ActiveProfile { get; set; }
    /// <summary>
    /// Index of a background image to load (0 to not load anything)
    /// </summary>
    [Option('i', "image-index", Required = false, HelpText = "Index of image to draw (0 for no image)")]
    public int? ImageIndex { get; set; }
    /// <summary>
    /// Background image scale (10-100, 100 - fill the window)
    /// </summary>
    [Option('a', "image-scale", Required = false, HelpText = "Background image scale in percent (10-100)")]
    public uint? ImageScale { get; set; }
}