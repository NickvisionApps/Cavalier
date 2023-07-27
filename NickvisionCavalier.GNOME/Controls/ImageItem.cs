using System;
using System.Threading.Tasks;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// Image item in background image selector
/// </summary>
public class ImageItem : Gtk.Overlay
{
    private readonly Gtk.Picture _picture;
    private readonly Gtk.Button _removeButton;
    private readonly int _index;
    
    /// <summary>
    /// Occurs when remove button was clicked
    /// </summary>
    public event Action<int>? OnRemoveImage;
    
    /// <summary>
    /// Construct ImageItem
    /// </summary>
    public ImageItem(string path, int index)
    {
        _index = index;
        SetMarginTop(2);
        SetMarginStart(2);
        SetMarginEnd(2);
        SetMarginBottom(2);
        _picture = Gtk.Picture.New();
        _picture.AddCssClass("cavalier-image");
        _picture.SetSizeRequest(160, 120);
        _picture.SetContentFit(Gtk.ContentFit.Cover);
        Task.Run(() =>
        {
            var texture = Gdk.Texture.NewFromFilename(path); // This is thread-safe
            GLib.Functions.IdleAdd(0, () =>
            {
                _picture.SetPaintable(texture);
                return false;
            });
        });
        SetChild(_picture);
        _removeButton = Gtk.Button.New();
        _removeButton.SetIconName("cross-symbolic");
        _removeButton.AddCssClass("circular");
        _removeButton.AddCssClass("osd");
        _removeButton.SetHalign(Gtk.Align.End);
        _removeButton.SetValign(Gtk.Align.Start);
        _removeButton.SetMarginTop(6);
        _removeButton.SetMarginStart(6);
        _removeButton.SetMarginEnd(6);
        _removeButton.SetMarginBottom(6);
        _removeButton.OnClicked += (sender, e) => OnRemoveImage?.Invoke(_index);
        AddOverlay(_removeButton);
    }
}