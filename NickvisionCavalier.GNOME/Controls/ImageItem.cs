using System;
using System.Threading.Tasks;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// Image item in background image selector
/// </summary>
public class ImageItem : Gtk.Overlay, IDisposable
{
    private readonly Gtk.Picture _picture;
    private readonly Gtk.Button _removeButton;
    private Gdk.Texture? _texture;
    private bool _disposed;

    /// <summary>
    /// Occurs when remove button was clicked
    /// </summary>
    public event Action<int>? OnRemoveImage;

    /// <summary>
    /// Construct ImageItem
    /// </summary>
    public ImageItem(string path, int index)
    {
        _disposed = false;
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
            _texture = Gdk.Texture.NewFromFilename(path); // This is thread-safe
            GLib.Functions.IdleAdd(0, () =>
            {
                _picture.SetPaintable(_texture);
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
        _removeButton.OnClicked += (sender, e) => OnRemoveImage?.Invoke(index);
        AddOverlay(_removeButton);
    }

    /// <summary>
    /// Finalizes the ImageItem object
    /// </summary>
    ~ImageItem() => Dispose(false);

    /// <summary>
    /// Frees resources used by the ImageItem object
    /// </summary>
    public new void Dispose()
    {
        Dispose(true);
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the ImageItem object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _texture?.Dispose();
            _picture.Dispose();
        }
        _disposed = true;
    }
}