using CommandLine;
using CommandLine.Text;
using Nickvision.Aura;
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
            if (!Directory.Exists($"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}images"))
            {
                Directory.CreateDirectory($"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}images");
                return result;
            }
            foreach (var file in Directory.GetFiles($"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}images"))
            {
                var extension = Path.GetExtension(file);
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
                Configuration.Current.AreaMargin = Math.Min(o.AreaMargin.Value, 40);
                updateCavalier = true;
            }
            if (o.Borderless.HasValue)
            {
                Configuration.Current.Borderless = o.Borderless.Value;
                updateCavalier = true;
            }
            if (o.SharpCorners.HasValue)
            {
                Configuration.Current.SharpCorners = o.SharpCorners.Value;
                updateCavalier = true;
            }
            if (o.BarPairs.HasValue)
            {
                Configuration.Current.BarPairs = Math.Max(3, Math.Min(o.BarPairs.Value, 50));
                updateCAVA = true;
            }
            if (o.Stereo.HasValue)
            {
                Configuration.Current.Stereo = o.Stereo.Value;
                updateCAVA = true;
            }
            if (o.ReverseOrder.HasValue)
            {
                Configuration.Current.ReverseOrder = o.ReverseOrder.Value;
                updateCavalier = true;
            }
            if (o.Direction.HasValue)
            {
                Configuration.Current.Direction = o.Direction.Value;
                updateCavalier = true;
            }
            if (o.ItemsOffset.HasValue)
            {
                Configuration.Current.ItemsOffset = Math.Min(o.ItemsOffset.Value, 20) / 100.0f;
                updateCavalier = true;
            }
            if (o.ItemsRoundness.HasValue)
            {
                Configuration.Current.ItemsRoundness = Math.Min(o.ItemsRoundness.Value, 100) / 100.0f;
                updateCavalier = true;
            }
            if (o.Filling.HasValue)
            {
                Configuration.Current.Filling = o.Filling.Value;
                updateCavalier = true;
            }
            if (o.LinesThickness.HasValue)
            {
                Configuration.Current.LinesThickness = Math.Min(o.LinesThickness.Value, 10);
                updateCavalier = true;
            }
            if (o.Mode.HasValue)
            {
                Configuration.Current.Mode = o.Mode.Value;
                updateCavalier = true;
            }
            if (o.Mirror.HasValue)
            {
                Configuration.Current.Mirror = o.Mirror.Value;
                updateCavalier = true;
            }
            if (o.ReverseMirror.HasValue)
            {
                Configuration.Current.ReverseMirror = o.ReverseMirror.Value;
                updateCavalier = true;
            }
            if (o.InnerRadius.HasValue)
            {
                Configuration.Current.InnerRadius = Math.Max(80, Math.Min(20, o.InnerRadius.Value)) / 100f;
                updateCavalier = true;
            }
            if (o.ActiveProfile.HasValue)
            {
                if (o.ActiveProfile.Value < Configuration.Current.ColorProfiles.Count)
                {
                    Configuration.Current.ActiveProfile = (int)o.ActiveProfile.Value;
                    updateCavalier = true;
                }
            }
            if (o.ImageIndex.HasValue)
            {
                if (o.ImageIndex.Value > -1 && o.ImageIndex.Value <= ImagesList.Count)
                {
                    Configuration.Current.ImageIndex = o.ImageIndex.Value - 1;
                    updateCavalier = true;
                }
            }
            if (o.ImageScale.HasValue)
            {
                Configuration.Current.ImageScale = Math.Max(0.1f, Math.Min(o.ImageScale.Value / 100f, 1f));
                updateCavalier = true;
            }
            if (o.Hearts)
            {
                Configuration.Current.Hearts = true;
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
    /// Inner circle radius ratio in circle modes (0.2-0.8)
    /// </summary>
    public float InnerRadius
    {
        get => Configuration.Current.InnerRadius;
    
        set => Configuration.Current.InnerRadius = value;
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
    public int ImageIndex
    {
        get => Configuration.Current.ImageIndex;

        set => Configuration.Current.ImageIndex = value;
    }

    /// <summary>
    /// Background image scale (0.1-1.0, 1.0 - fill the window)
    /// </summary>
    public float ImageScale
    {
        get => Configuration.Current.ImageScale;
        
        set => Configuration.Current.ImageScale = value;
    }
    
    /// <summary>
    /// Whether to replace Spine mode with Hearts mode (easter egg)
    /// </summary>
    /// <remarks>Suggested by my beloved Xenia &lt;3</remarks>
    public bool Hearts => Configuration.Current.Hearts;

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
        while (File.Exists($"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}{filename}"))
        {
            i++;
            filename = $"{Path.GetFileNameWithoutExtension(baseFilename)}-{i}{Path.GetExtension(baseFilename)}";
        }
        File.Copy(path, $"{ConfigurationLoader.ConfigDir}{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}{filename}");
    }
}
