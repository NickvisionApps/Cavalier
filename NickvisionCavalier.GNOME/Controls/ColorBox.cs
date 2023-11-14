using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Models;
using System;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// Arguments for color deletion and editing events
/// </summary>
public class ColorEventArgs : EventArgs
{
    /// <summary>
    /// The type of the color
    /// </summary>
    public ColorType Type { get; init; }
    /// <summary>
    /// The index of the color in the profile's list
    /// </summary>
    public int Index { get; init; }
    /// <summary>
    /// The color as a string (#aarrggbb)
    /// </summary>
    public string Color { get; init; }

    /// <summary>
    /// Creates ColorEventArgs
    /// </summary>
    /// <param name="type">Color type (background or foreground)</param>
    /// <param name="index">Color index in profile's list</param>
    /// <param name="color">Color string (#aarrggbb)</param>
    public ColorEventArgs(ColorType type, int index, string color = "")
    {
        Type = type;
        Index = index;
        Color = color;
    }
}

/// <summary>
/// A widget to control a color
/// </summary>
public partial class ColorBox : Gtk.Box
{
    /// <summary>
    /// Occurs on color deletion request
    /// </summary>
    public event EventHandler<ColorEventArgs>? OnDelete;
    /// <summary>
    /// Occurs on color editing request
    /// </summary>
    public event EventHandler<ColorEventArgs>? OnEdit;

    /// <summary>
    /// Creates color box
    /// </summary>
    /// <param name="color">Color string (#aarrggbb)</param>
    /// <param name="type">Color type (background or foreground)</param>
    /// <param name="index">Color index in profile's list</param>
    /// <param name="canDelete">Whether the color can be deleted</param>
    public ColorBox(string color, ColorType type, int index, bool canDelete = true)
    {
        SetHalign(Gtk.Align.Center);
        SetSpacing(4);
        SetMarginBottom(8);
        var colorButton = Gtk.ColorDialogButton.New(Gtk.ColorDialog.New());
        Append(colorButton);
        colorButton.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "rgba")
            {
                var extColor = colorButton.GetExtRgba();
                if (extColor.Alpha <= 0.0001f)
                {
                    extColor.Red = 0;
                    extColor.Green = 0;
                    extColor.Blue = 0;
                }
                OnEdit?.Invoke(this, new ColorEventArgs(type, index, $"#{((int)(extColor.Alpha * 255)).ToString("x2")}{((int)(extColor.Red * 255)).ToString("x2")}{((int)(extColor.Green * 255)).ToString("x2")}{((int)(extColor.Blue * 255)).ToString("x2")}"));
            }
        };
        var rgbaColor = $"#{color.Substring(3)}{color.Substring(1, 2)}";
        GdkHelpers.RGBA.Parse(out var extNullColor, rgbaColor);
        if (extNullColor != null)
        {
            var extColor = extNullColor.Value;
            if (extColor.Alpha == 0)
            {
                extColor.Alpha = 0.0001f; // Work around bug when button doesn't show color if alpha is 0
            }
            colorButton.SetExtRgba(extColor);
        }
        var deleteButton = Gtk.Button.New();
        deleteButton.AddCssClass("circular");
        deleteButton.SetIconName("cross-symbolic");
        deleteButton.SetSensitive(canDelete);
        Append(deleteButton);
        deleteButton.OnClicked += (sender, e) => OnDelete?.Invoke(this, new ColorEventArgs(type, index));
    }
}