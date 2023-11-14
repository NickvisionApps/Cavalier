using System;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// A widget to control a profile
/// </summary>
public class ProfileBox : Gtk.Box
{
    public int Index { get; set; }

    public event EventHandler<int>? OnDelete;

    /// <summary>
    /// Creates ProfileBox
    /// </summary>
    /// <param name="name">Profile name</param>
    /// <param name="index">Profile index</param>
    public ProfileBox(string name, int index)
    {
        Index = index;
        SetSizeRequest(-1, 34);
        SetSpacing(4);
        SetMarginTop(8);
        SetMarginStart(8);
        SetMarginEnd(8);
        SetMarginBottom(8);
        var label = Gtk.Label.New(name);
        label.SetEllipsize(Pango.EllipsizeMode.End);
        label.SetHexpand(true);
        label.SetHalign(Gtk.Align.Start);
        Append(label);
        if (index > 0)
        {
            var deleteButton = Gtk.Button.New();
            deleteButton.AddCssClass("circular");
            deleteButton.SetIconName("cross-symbolic");
            Append(deleteButton);
            deleteButton.OnClicked += (sender, e) => OnDelete?.Invoke(this, Index);
        }
    }
}