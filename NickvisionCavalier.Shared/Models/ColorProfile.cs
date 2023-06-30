using System;
using System.Collections.Generic;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.Shared.Models;

/// <summary>
/// Cavalier Color Profile
/// </summary>
public class ColorProfile : ICloneable
{
    /// <summary>
    /// Name of the profile
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// List of foreground colors, strings in formst "#aarrggbb"
    /// </summary>
    public List<string> FgColors { get; set; }
    /// <summary>
    /// List of background colors, strings in format "#aarrggbb"
    /// </summary>
    public List<string> BgColors { get; set; }
    /// <summary>
    /// Application theme
    /// </summary>
    public Theme Theme { get; set; }
    
    /// <summary>
    /// Creates default profile
    /// </summary>
    public ColorProfile()
    {
        Name = _("Default");
        FgColors = new List<string> { "#ff3584e4" };
        BgColors = new List<string> { "#ff242424" };
        Theme = Theme.Dark;
    }

    /// <summary>
    /// Clones the color profile
    /// </summary>
    /// <returns>A new color profile</returns>
    public object Clone()
    {
        return new ColorProfile()
        {
            Name = Name,
            FgColors = FgColors,
            BgColors = BgColors,
            Theme = Theme
        };
    }
}