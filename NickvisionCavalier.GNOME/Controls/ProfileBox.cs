using System;

namespace NickvisionCavalier.GNOME.Controls;

public class ProfileBox : Gtk.Box
{
    public int Index;
    public event EventHandler<int>? OnDelete;
    
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