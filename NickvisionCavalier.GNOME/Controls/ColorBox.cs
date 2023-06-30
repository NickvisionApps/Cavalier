using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Runtime.InteropServices;

namespace NickvisionCavalier.GNOME.Controls;

public class ColorEventArgs : EventArgs
{
    public readonly ColorType Type;
    public readonly int Index;
    public readonly string Color;
    
    public ColorEventArgs(ColorType type, int index, string color = "")
    {
        Type = type;
        Index = index;
        Color = color;
    }
}

public partial class ColorBox : Gtk.Box
{
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.I1)]
    private static partial bool gdk_rgba_parse(out Color rgba, string spec);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_button_new(nint dialog);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_rgba(nint button, ref Color rgba);

    [DllImport("libadwaita-1.so.0")]
    static extern ref Color gtk_color_dialog_button_get_rgba(nint button);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_new();

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_box_append(nint box, nint widget);

    private delegate void GCallback();
    
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial ulong g_signal_connect_data(nint instance, string detailed_signal, GCallback c_handler, nint data, nint destroy_data, int connect_flags);
    
    private readonly GCallback _colorButtonNotify;
    
    public event EventHandler<ColorEventArgs>? OnDelete;
    public event EventHandler<ColorEventArgs>? OnEdit;
    
    public ColorBox(string color, ColorType type, int index, bool canDelete = true)
    {
        SetHalign(Gtk.Align.Center);
        SetSpacing(4);
        SetMarginBottom(8);
        var colorDialog = gtk_color_dialog_new();
        var colorButton = gtk_color_dialog_button_new(colorDialog);
        gtk_box_append(Handle, colorButton);
        _colorButtonNotify = () =>
        {
            var color = gtk_color_dialog_button_get_rgba(colorButton);
            if (color.Alpha <= 0.0001f)
            {
                color.Red = 0;
                color.Green = 0;
                color.Blue = 0;
            }
            OnEdit?.Invoke(this, new ColorEventArgs(type, index, $"#{((int)(color.Alpha * 255)).ToString("x2")}{((int)(color.Red * 255)).ToString("x2")}{((int)(color.Green * 255)).ToString("x2")}{((int)(color.Blue * 255)).ToString("x2")}"));
        };
        g_signal_connect_data(colorButton, "notify::rgba", _colorButtonNotify, IntPtr.Zero, IntPtr.Zero, 0);
        var rgbaColor = $"#{color.Substring(3)}{color.Substring(1, 2)}";
        Color gdkColor;
        gdk_rgba_parse(out gdkColor, rgbaColor);
        if (gdkColor.Alpha == 0)
        {
            gdkColor.Alpha = 0.0001f; // Work around bug when button doesn't show color if alpha is 0
        }
        gtk_color_dialog_button_set_rgba(colorButton, ref gdkColor);
        var deleteButton = Gtk.Button.New();
        deleteButton.AddCssClass("circular");
        deleteButton.SetIconName("cross-symbolic");
        deleteButton.SetSensitive(canDelete);
        Append(deleteButton);
        deleteButton.OnClicked += (sender, e) => OnDelete?.Invoke(this, new ColorEventArgs(type, index));
    }
}