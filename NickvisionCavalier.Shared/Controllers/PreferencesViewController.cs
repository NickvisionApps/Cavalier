using NickvisionCavalier.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace NickvisionCavalier.Shared.Controllers;

/// <summary>
/// A controller for a PreferencesView
/// </summary>
public class PreferencesViewController
{
    /// <summary>
    /// Command-line parser
    /// </summary>
    private readonly Parser _parser;
    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public string ID => Aura.Active.AppInfo.ID;
    /// <summary>
    /// The list of images that can be used as background images
    /// </summary>
    /// <returns>List of paths</returns>
    public List<string> ImagesList
    {
        get
        {
            var result = new List<string>();
            var imagesDir = $"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}images";
            if (!Directory.Exists(imagesDir))
            {
                Directory.CreateDirectory(imagesDir);
                return result;
            }
            foreach (var file in Directory.GetFiles(imagesDir))
            {
                var extension = Path.GetExtension(file).ToLower();
                if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
                {
                    result.Add(file);
                }
            }
            result.Sort();
            return result;
        }
    }

    /// <summary>
    /// Occurs when the view needs to be updated (instant settings)
    /// </summary>
    public event EventHandler? OnUpdateViewInstant;
    /// <summary>
    /// Occurs when the view needs to be updated (CAVA settings)
    /// </summary>
    public event EventHandler? OnUpdateViewCAVA;
    /// <summary>
    /// Occurs when window settings were changed from the view
    /// </summary>
    public event EventHandler? OnWindowSettingsChanged;
    /// <summary>
    /// Occurs when CAVA settings were changed from the view
    /// </summary>
    public event EventHandler? OnCAVASettingsChanged;
    /// <summary>
    /// Occurs when Help screen needs to be shown
    /// </summary>
    public event EventHandler<string>? OnShowHelpScreen;

    /// <summary>
    /// Constructs a PreferencesViewController
    /// </summary>
    internal PreferencesViewController()
    {
        _parser = new Parser(with =>
        {
            with.AutoVersion = false;
            with.HelpWriter = null;
        });
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
    /// Drawing area X offset (-0.5-0.5)
    /// </summary>
    public float AreaOffsetX
    {
        get => Configuration.Current.AreaOffsetX;

        set => Configuration.Current.AreaOffsetX = value;
    }

    /// <summary>
    /// Drawing area Y offset (-0.5-0.5)
    /// </summary>
    public float AreaOffsetY
    {
        get => Configuration.Current.AreaOffsetY;

        set => Configuration.Current.AreaOffsetY = value;
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
    /// Noise reduction value (0.15-0.95)
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
    /// Inner circle radius ratio in circle modes (0.2-0.8)
    /// </summary>
    public float InnerRadius
    {
        get => Configuration.Current.InnerRadius;

        set => Configuration.Current.InnerRadius = value;
    }

    /// <summary>
    /// Rotation angle in radians in circle modes (0-2PI)
    /// </summary>
    public float Rotation
    {
        get => Configuration.Current.Rotation;

        set => Configuration.Current.Rotation = value;
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
    /// Index of a background image to load (-1 to not load anything)
    /// </summary>
    public int BgImageIndex
    {
        get => Configuration.Current.BgImageIndex;

        set => Configuration.Current.BgImageIndex = value;
    }

    /// <summary>
    /// Background image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float BgImageScale
    {
        get => Configuration.Current.BgImageScale;

        set => Configuration.Current.BgImageScale = value;
    }

    /// <summary>
    /// Background image transparency (0.1-1.0, 1.0 - fully opaque)
    /// </summary>
    public float BgImageAlpha
    {
        get => Configuration.Current.BgImageAlpha;

        set => Configuration.Current.BgImageAlpha = value;
    }

    /// <summary>
    /// Index of a foreground image to load (-1 to not load anything)
    /// </summary>
    public int FgImageIndex
    {
        get => Configuration.Current.FgImageIndex;

        set => Configuration.Current.FgImageIndex = value;
    }

    /// <summary>
    /// Foreground image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float FgImageScale
    {
        get => Configuration.Current.FgImageScale;

        set => Configuration.Current.FgImageScale = value;
    }

    /// <summary>
    /// Foreground image transparency (0.1-1.0, 1.0 - fully opaque)
    /// </summary>
    public float FgImageAlpha
    {
        get => Configuration.Current.FgImageAlpha;

        set => Configuration.Current.FgImageAlpha = value;
    }

    /// <summary>
    /// Whether to replace Spine mode with Hearts mode (easter egg)
    /// </summary>
    /// <remarks>Suggested by my beloved Xenia &lt;3</remarks>
    public bool Hearts
    {
        get => Configuration.Current.Hearts;

        private set => Configuration.Current.Hearts = value;
    }

    /// <summary>
    /// Process command-line arguments passed when starting the app or from other instances
    /// </summary>
    public void HandleCommandLine(object? sender, string[] args)
    {
        var parserResult = _parser.ParseArguments<CmdOptions>(args);
        parserResult.WithParsed((o) =>
        {
            var updateCavalier = false;
            var updateCAVA = false;
            if (o.AreaMargin.HasValue)
            {
                AreaMargin = Math.Min(o.AreaMargin.Value, 40);
                updateCavalier = true;
            }
            if (o.AreaOffsetX.HasValue)
            {
                AreaOffsetX = Math.Max(-50, Math.Min(o.AreaOffsetX.Value, 50)) / 100f;
                updateCavalier = true;
            }
            if (o.AreaOffsetY.HasValue)
            {
                AreaOffsetY = Math.Max(-50, Math.Min(o.AreaOffsetY.Value, 50)) / 100f;
                updateCavalier = true;
            }
            if (o.AreaMargin.HasValue)
            {
                AreaMargin = Math.Min(o.AreaMargin.Value, 40);
                updateCavalier = true;
            }
            if (o.Borderless.HasValue)
            {
                Borderless = o.Borderless.Value;
                updateCavalier = true;
            }
            if (o.SharpCorners.HasValue)
            {
                SharpCorners = o.SharpCorners.Value;
                updateCavalier = true;
            }
            if (o.BarPairs.HasValue)
            {
                BarPairs = Math.Max(3, Math.Min(o.BarPairs.Value, 50));
                updateCAVA = true;
            }
            if (o.Stereo.HasValue)
            {
                Stereo = o.Stereo.Value;
                updateCAVA = true;
            }
            if (o.ReverseOrder.HasValue)
            {
                ReverseOrder = o.ReverseOrder.Value;
                updateCavalier = true;
            }
            if (o.Direction.HasValue)
            {
                Direction = o.Direction.Value;
                updateCavalier = true;
            }
            if (o.ItemsOffset.HasValue)
            {
                ItemsOffset = Math.Min(o.ItemsOffset.Value, 20) / 100.0f;
                updateCavalier = true;
            }
            if (o.ItemsRoundness.HasValue)
            {
                ItemsRoundness = Math.Min(o.ItemsRoundness.Value, 100) / 100.0f;
                updateCavalier = true;
            }
            if (o.Filling.HasValue)
            {
                Filling = o.Filling.Value;
                updateCavalier = true;
            }
            if (o.LinesThickness.HasValue)
            {
                LinesThickness = Math.Min(o.LinesThickness.Value, 10);
                updateCavalier = true;
            }
            if (o.Mode.HasValue)
            {
                Mode = o.Mode.Value;
                updateCavalier = true;
            }
            if (o.Mirror.HasValue)
            {
                Mirror = o.Mirror.Value;
                updateCavalier = true;
            }
            if (o.ReverseMirror.HasValue)
            {
                ReverseMirror = o.ReverseMirror.Value;
                updateCavalier = true;
            }
            if (o.InnerRadius.HasValue)
            {
                InnerRadius = Math.Max(80, Math.Min(20, o.InnerRadius.Value)) / 100f;
                updateCavalier = true;
            }
            if (o.Rotation.HasValue)
            {
                Rotation = Math.Max(360, o.InnerRadius.Value) / 360f * (float)Math.PI * 2;
                updateCavalier = true;
            }
            if (o.ActiveProfile.HasValue)
            {
                if (o.ActiveProfile.Value < Configuration.Current.ColorProfiles.Count)
                {
                    ActiveProfile = (int)o.ActiveProfile.Value;
                    updateCavalier = true;
                }
            }
            if (o.BgImageIndex.HasValue)
            {
                if (o.BgImageIndex.Value > -1 && o.BgImageIndex.Value <= ImagesList.Count)
                {
                    BgImageIndex = o.BgImageIndex.Value - 1;
                    updateCavalier = true;
                }
            }
            if (o.BgImageScale.HasValue)
            {
                BgImageScale = Math.Max(0.1f, Math.Min(o.BgImageScale.Value / 100f, 1f));
                updateCavalier = true;
            }
            if (o.BgImageAlpha.HasValue)
            {
                BgImageAlpha = Math.Max(0.1f, Math.Min(o.BgImageAlpha.Value / 100f, 1f));
                updateCavalier = true;
            }
            if (o.FgImageIndex.HasValue)
            {
                if (o.FgImageIndex.Value > -1 && o.FgImageIndex.Value <= ImagesList.Count)
                {
                    FgImageIndex = o.FgImageIndex.Value - 1;
                    updateCavalier = true;
                }
            }
            if (o.FgImageScale.HasValue)
            {
                FgImageScale = Math.Max(0.1f, Math.Min(o.FgImageScale.Value / 100f, 1f));
                updateCavalier = true;
            }
            if (o.FgImageAlpha.HasValue)
            {
                FgImageAlpha = Math.Max(0.1f, Math.Min(o.FgImageAlpha.Value / 100f, 1f));
                updateCavalier = true;
            }
            if (o.Hearts)
            {
                Hearts = true;
                updateCavalier = true;
            }
            if (updateCavalier)
            {
                OnUpdateViewInstant?.Invoke(this, EventArgs.Empty);
            }
            if (updateCAVA)
            {
                OnUpdateViewCAVA?.Invoke(this, EventArgs.Empty);
            }
        }).WithNotParsed(_ =>
        {
            var help = GenerateHelp(parserResult);
            if (sender is MainWindowController)
            {
                // Help screen was caused by first instance on start, let's show and exit
                Console.WriteLine(help);
                Environment.Exit(1);
            }
            OnShowHelpScreen?.Invoke(this, help);
        });
    }

    /// <summary>
    /// Create help text
    /// </summary>
    /// <param name="result">Command line parser result</param>
    /// <returns>Help text</returns>
    private string GenerateHelp<T>(ParserResult<T> result)
    {
        var helpText = HelpText.AutoBuild(result, h =>
        {
            h.AdditionalNewLineAfterOption = true;
            h.Heading = $"{Aura.Active.AppInfo.ShortName} {Aura.Active.AppInfo.Version}";
            h.Copyright = "Copyright (c) 2023 Nickvision";
            h.AutoVersion = false;
            return HelpText.DefaultParsingErrorsHandler(result, h);
        }, e => e);
        return helpText;
    }

    /// <summary>
    /// Saves the configuration to disk
    /// </summary>
    public void Save() => Aura.Active.SaveConfig("config");

    /// <summary>
    /// Occurs when a window's setting has changed
    /// </summary>
    public void ChangeWindowSettings() => OnWindowSettingsChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Occurs when a CAVA setting has changed
    /// </summary>
    public void ChangeCAVASettings() => OnCAVASettingsChanged?.Invoke(this, EventArgs.Empty);

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

    /// <summary>
    /// Copy image from given path to Cavalier's images folder
    /// </summary>
    public void AddImage(string path)
    {
        var baseFilename = Path.GetFileName(path);
        var filename = baseFilename;
        var i = 0;
        while (File.Exists($"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}{filename}"))
        {
            i++;
            filename = $"{Path.GetFileNameWithoutExtension(baseFilename)}-{i}{Path.GetExtension(baseFilename)}";
        }
        File.Copy(path, $"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}{filename}");
    }
}
