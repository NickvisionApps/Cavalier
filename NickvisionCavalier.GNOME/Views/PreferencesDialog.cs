using NickvisionCavalier.GNOME.Controls;
using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.Runtime.InteropServices;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private delegate void GAsyncReadyCallback(nint source_object, nint res, nint data);

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_new();
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_choose_rgba(nint dialog, nint parent, nint initial_color, nint cancellable, GAsyncReadyCallback callback, nint user_data);
    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial nint gtk_color_dialog_choose_rgba_finish(nint dialog, nint result, nint error);

    private readonly GAsyncReadyCallback _fgColorDialogCallback;
    private readonly GAsyncReadyCallback _bgColorDialogCallback;
    private readonly nint _colorDialog;
    private readonly PreferencesViewController _controller;

    [Gtk.Connect] private readonly Gtk.CheckButton _waveCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _levelsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _particlesCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _barsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _spineCheckButton;
    [Gtk.Connect] private readonly Adw.ComboRow _mirrorRow;
    [Gtk.Connect] private readonly Gtk.Scale _marginScale;
    [Gtk.Connect] private readonly Adw.ComboRow _directionRow;
    [Gtk.Connect] private readonly Adw.ActionRow _offsetRow;
    [Gtk.Connect] private readonly Gtk.Scale _offsetScale;
    [Gtk.Connect] private readonly Adw.ActionRow _roundnessRow;
    [Gtk.Connect] private readonly Gtk.Scale _roundnessScale;
    [Gtk.Connect] private readonly Gtk.Switch _fillingSwitch;
    [Gtk.Connect] private readonly Adw.ActionRow _thicknessRow;
    [Gtk.Connect] private readonly Gtk.Scale _thicknessScale;
    [Gtk.Connect] private readonly Gtk.Switch _borderlessSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _sharpCornersSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _windowControlsSwitch;
    [Gtk.Connect] private readonly Gtk.Switch _autohideHeaderSwitch;
    [Gtk.Connect] private readonly Adw.ComboRow _framerateRow;
    [Gtk.Connect] private readonly Gtk.Scale _barsScale;
    [Gtk.Connect] private readonly Gtk.Switch _autosensSwitch;
    [Gtk.Connect] private readonly Gtk.Scale _sensitivityScale;
    [Gtk.Connect] private readonly Gtk.ToggleButton _stereoButton;
    [Gtk.Connect] private readonly Gtk.Switch _monstercatSwitch;
    [Gtk.Connect] private readonly Gtk.Scale _noiseReductionScale;
    [Gtk.Connect] private readonly Gtk.Switch _reverseSwitch;
    [Gtk.Connect] private readonly Gtk.ListBox _profilesList;
    [Gtk.Connect] private readonly Gtk.Button _addProfileButton;
    [Gtk.Connect] private readonly Gtk.ToggleButton _lightThemeButton;
    [Gtk.Connect] private readonly Gtk.Grid _colorsGrid;
    [Gtk.Connect] private readonly Gtk.Button _addFgColorButton;
    [Gtk.Connect] private readonly Gtk.Button _addBgColorButton;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller) : base(builder.GetPointer("_root"), false)
    {
        //Window Settings
        _controller = controller;
        SetIconName(_controller.AppInfo.ID);
        //Build UI
        builder.Connect(this);
        OnCloseRequest += (sender, e) =>
        {
            _controller.Save();
            return false;
        };
        _waveCheckButton.OnToggled += (sender, e) =>
        {
            if (_waveCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.WaveBox;
                _offsetRow.SetSensitive(false);
                _roundnessRow.SetSensitive(false);
            }
        };
        _levelsCheckButton.OnToggled += (sender, e) =>
        {
            if (_levelsCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.LevelsBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        _particlesCheckButton.OnToggled += (sender, e) =>
        {
            if (_particlesCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.ParticlesBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        _barsCheckButton.OnToggled += (sender, e) =>
        {
            if (_barsCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.BarsBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(false);
            }
        };
        _spineCheckButton.OnToggled += (sender, e) =>
        {
            if (_spineCheckButton.GetActive())
            {
                _controller.Mode = DrawingMode.SpineBox;
                _offsetRow.SetSensitive(true);
                _roundnessRow.SetSensitive(true);
            }
        };
        switch (_controller.Mode)
        {
            case DrawingMode.WaveBox:
                _waveCheckButton.SetActive(true);
                break;
            case DrawingMode.LevelsBox:
                _levelsCheckButton.SetActive(true);
                break;
            case DrawingMode.ParticlesBox:
                _particlesCheckButton.SetActive(true);
                break;
            case DrawingMode.BarsBox:
                _barsCheckButton.SetActive(true);
                break;
            case DrawingMode.SpineBox:
                _spineCheckButton.SetActive(true);
                break;
        }
        if (_controller.Stereo)
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("Full"), _("Split Channels") }));
            _mirrorRow.SetSelected((uint)_controller.Mirror);
        }
        else
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("On") }));
            if (_controller.Mirror == Mirror.SplitChannels)
            {
                _mirrorRow.SetSelected(1u);
            }
            else
            {
                _mirrorRow.SetSelected((uint)_controller.Mirror);
            }
        }
        _mirrorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Mirror = (Mirror)_mirrorRow.GetSelected();
            }
        };
        _marginScale.SetValue((int)_controller.AreaMargin);
        _marginScale.OnValueChanged += (sender, e) =>
        {
            _controller.AreaMargin = (uint)_marginScale.GetValue();
        };
        _directionRow.SetSelected((uint)_controller.Direction);
        _directionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Direction = (DrawingDirection)_directionRow.GetSelected();
            }
        };
        _offsetScale.SetValue((int)(_controller.ItemsOffset * 100));
        _offsetScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsOffset = (float)_offsetScale.GetValue() / 100.0f;
        };
        _roundnessScale.SetValue((int)(_controller.ItemsRoundness * 100));
        _roundnessScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsRoundness = (float)_roundnessScale.GetValue() / 100.0f;
        };
        _fillingSwitch.SetActive(_controller.Filling);
        _fillingSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Filling = _fillingSwitch.GetActive();
            }
        };
        _thicknessRow.SetSensitive(!_fillingSwitch.GetActive());
        _thicknessScale.SetValue((int)_controller.LinesThickness);
        _thicknessScale.OnValueChanged += (sender, e) =>
        {
            _controller.LinesThickness = (float)_thicknessScale.GetValue();
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
        _framerateRow.SetSelected(_controller.Framerate / 30u - 1u);
        _framerateRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Framerate = (_framerateRow.GetSelected() + 1u) * 30u;
                _controller.ChangeCavaSettings();
            }
        };
        _barsScale.SetValue((int)_controller.BarPairs * 2);
        _barsScale.OnValueChanged += (sender, e) =>
        {
            if (_barsScale.GetValue() % 2 != 0)
            {
                _barsScale.SetValue(_barsScale.GetValue() - 1);
                return;
            }
            _controller.BarPairs = (uint)(_barsScale.GetValue() / 2);
            _controller.ChangeCavaSettings();
        };
        _autosensSwitch.SetActive(_controller.Autosens);
        _autosensSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Autosens = _autosensSwitch.GetActive();
                _controller.ChangeCavaSettings();
            }
        };
        _sensitivityScale.SetValue((int)_controller.Sensitivity);
        _sensitivityScale.OnValueChanged += (sender, e) =>
        {
            _controller.Sensitivity = (uint)_sensitivityScale.GetValue();
            _controller.ChangeCavaSettings();
        };
        _stereoButton.SetActive(_controller.Stereo);
        _stereoButton.OnToggled += (sender, e) =>
        {
            if (_stereoButton.GetActive())
            {
                _controller.Stereo = true;
                _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("Full"), _("Split Channels") }));
            }
            else
            {
                _controller.Stereo = false;
                _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("On") }));
            }
            _controller.ChangeCavaSettings();
        };
        _monstercatSwitch.SetActive(_controller.Monstercat);
        _monstercatSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Monstercat = _monstercatSwitch.GetActive();
                _controller.ChangeCavaSettings();
            }
        };
        _noiseReductionScale.SetValue((double)_controller.NoiseReduction);
        _noiseReductionScale.GetFirstChild().SetMarginBottom(12);
        _noiseReductionScale.AddMark(0.77, Gtk.PositionType.Bottom, null);
        _noiseReductionScale.OnValueChanged += (sender, e) =>
        {
            _controller.NoiseReduction = (float)_noiseReductionScale.GetValue();
            _controller.ChangeCavaSettings();
        };
        _reverseSwitch.SetActive(_controller.ReverseOrder);
        _reverseSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ReverseOrder = _reverseSwitch.GetActive();
            }
        };
        _profilesList.OnRowSelected += (sender, e) =>
        {
            if (e.Row != null)
            {
                _controller.ActiveProfile = ((ProfileBox)e.Row.GetChild()).Index;
                _lightThemeButton.SetActive(_controller.ColorProfiles[_controller.ActiveProfile].Theme == Theme.Light);
                UpdateColorsGrid();
            }
        };
        _addProfileButton.OnClicked += OnAddProfile;
        _lightThemeButton.OnToggled += (sender, e) =>
        {
            _controller.ColorProfiles[_controller.ActiveProfile].Theme = _lightThemeButton.GetActive() ? Theme.Light : Theme.Dark;
            _controller.ChangeWindowSettings();
        };
        _colorDialog = gtk_color_dialog_new();
        _fgColorDialogCallback = (source, res, data) =>
        {
            var colorPtr = gtk_color_dialog_choose_rgba_finish(_colorDialog, res, IntPtr.Zero);
            if (colorPtr != IntPtr.Zero)
            {
                var color = (Color)Marshal.PtrToStructure(colorPtr, typeof(Color));
                if (color.Alpha <= 0.0001f)
                {
                    color.Red = 0;
                    color.Green = 0;
                    color.Blue = 0;
                }
                _controller.AddColor(ColorType.Foreground, $"#{((int)(color.Alpha * 255)).ToString("x2")}{((int)(color.Red * 255)).ToString("x2")}{((int)(color.Green * 255)).ToString("x2")}{((int)(color.Blue * 255)).ToString("x2")}");
                UpdateColorsGrid();
            }
        };
        _bgColorDialogCallback = (source, res, data) =>
        {
            var colorPtr = gtk_color_dialog_choose_rgba_finish(_colorDialog, res, IntPtr.Zero);
            if (colorPtr != IntPtr.Zero)
            {
                var color = (Color)Marshal.PtrToStructure(colorPtr, typeof(Color));
                if (color.Alpha <= 0.0001f)
                {
                    color.Red = 0;
                    color.Green = 0;
                    color.Blue = 0;
                }
                _controller.AddColor(ColorType.Background, $"#{((int)(color.Alpha * 255)).ToString("x2")}{((int)(color.Red * 255)).ToString("x2")}{((int)(color.Green * 255)).ToString("x2")}{((int)(color.Blue * 255)).ToString("x2")}");
                UpdateColorsGrid();
            }
        };
        _addFgColorButton.OnClicked += (sender, e) => AddColor(ColorType.Foreground);
        _addBgColorButton.OnClicked += (sender, e) => AddColor(ColorType.Background);
        UpdateColorProfiles();
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    public PreferencesDialog(PreferencesViewController controller) : this(Builder.FromFile("preferences_dialog.ui"), controller)
    {
    }

    private void UpdateColorProfiles()
    {
        _profilesList.SelectRow(null);
        while (_profilesList.GetRowAtIndex(0) != null)
        {
            _profilesList.Remove(_profilesList.GetRowAtIndex(0));
        }
        for (var i = 0; i < _controller.ColorProfiles.Count; i++)
        {
            var profileBox = new ProfileBox(_controller.ColorProfiles[i].Name, i);
            _profilesList.Append(profileBox);
            profileBox.OnDelete += OnDeleteProfile;
        }
        _profilesList.SelectRow(_profilesList.GetRowAtIndex(_controller.ActiveProfile));
    }

    private void OnAddProfile(object sender, EventArgs e)
    {
        var dialog = new AddProfileDialog(this, _controller.AppInfo.ID);
        dialog.OnDialogClosed += (sender, response) =>
        {
            if (response == MessageDialogResponse.Suggested)
            {
                _controller.AddColorProfile(dialog.ProfileName);
                UpdateColorProfiles();
            }
        };
        dialog.Present();
    }

    private void OnDeleteProfile(object sender, int index)
    {
        var dialog = new MessageDialog(
            this, _controller.AppInfo.ID,
            _("Delete Profile"), _("Are you sure you want to delete profile \"{0}\"?", _controller.ColorProfiles[index].Name),
            _("Cancel"), _("Delete"));
        dialog.OnDialogClosed += (sender, response) =>
        {
            if (response == MessageDialogResponse.Destructive)
            {
                _controller.ActiveProfile -= 1;
                _controller.ColorProfiles.RemoveAt(index);
                UpdateColorProfiles();
            }
        };
        dialog.Present();
    }

    private void UpdateColorsGrid()
    {
        while (_colorsGrid.GetChildAt(0, 1) != null || _colorsGrid.GetChildAt(1, 1) != null)
        {
            _colorsGrid.RemoveRow(1);
        }
        for (var i = 0; i < _controller.ColorProfiles[_controller.ActiveProfile].FgColors.Count; i++)
        {
            var colorButton = new ColorBox(
                _controller.ColorProfiles[_controller.ActiveProfile].FgColors[i],
                ColorType.Foreground, i, i != 0);
            colorButton.OnEdit += OnEditColor;
            colorButton.OnDelete += OnDeleteColor;
            _colorsGrid.Attach(colorButton, 0, i + 1, 1, 1);
        }
        for (var i = 0; i < _controller.ColorProfiles[_controller.ActiveProfile].BgColors.Count; i++)
        {
            var colorButton = new ColorBox(
                _controller.ColorProfiles[_controller.ActiveProfile].BgColors[i],
                ColorType.Background, i, i != 0);
            colorButton.OnEdit += OnEditColor;
            colorButton.OnDelete += OnDeleteColor;
            _colorsGrid.Attach(colorButton, 1, i + 1, 1, 1);
        }
    }

    private void AddColor(ColorType type) => gtk_color_dialog_choose_rgba(_colorDialog, Handle, IntPtr.Zero, IntPtr.Zero, type == ColorType.Foreground ? _fgColorDialogCallback : _bgColorDialogCallback, IntPtr.Zero);

    private void OnEditColor(object sender, ColorEventArgs e) => _controller.EditColor(e.Type, e.Index, e.Color);

    private void OnDeleteColor(object sender, ColorEventArgs e)
    {
        _controller.DeleteColor(e.Type, e.Index);
        UpdateColorsGrid();
    }
}
