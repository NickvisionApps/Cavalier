using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private readonly PreferencesViewController _controller;
    private readonly Adw.Application _application;

    [Gtk.Connect] private readonly Gtk.Scale _marginScale;
    [Gtk.Connect] private readonly Gtk.Switch _borderlessSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _sharpCornersSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _windowControlsSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _autohideHeaderSwitch;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        _marginScale.SetValue((int)_controller.AreaMargin);
        _marginScale.OnValueChanged += (sender, e) =>
        {
            _controller.AreaMargin = (uint)_marginScale.GetValue();
            _controller.ChangeWindowSettings();
        };
        _borderlessSwitch.SetActive(_controller.Borderless);
        _borderlessSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Borderless = _borderlessSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _sharpCornersSwitch.SetActive(_controller.SharpCorners);
        _sharpCornersSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.SharpCorners = _sharpCornersSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _windowControlsSwitch.SetActive(_controller.ShowControls);
        _windowControlsSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ShowControls = _windowControlsSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _autohideHeaderSwitch.SetActive(_controller.AutohideHeader);
        _autohideHeaderSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.AutohideHeader = _autohideHeaderSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    /// <param name="application">Adw.Application</param>
    /// <param name="parent">Gtk.Window</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application, Gtk.Window parent) : this(Builder.FromFile("preferences_dialog.ui"), controller, application, parent)
    {
    }
}
