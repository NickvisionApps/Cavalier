using NickvisionCavalier.GNOME.Controls;
using NickvisionCavalier.GNOME.Helpers;
using NickvisionCavalier.Shared.Controllers;
using NickvisionCavalier.Shared.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using static Nickvision.GirExt.GtkExt;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Views;

/// <summary>
/// The PreferencesDialog for the application
/// </summary>
public partial class PreferencesDialog : Adw.PreferencesWindow
{
    private bool _avoidCAVAReload;
    private bool _removingImages;
    private readonly Gtk.ColorDialog _colorDialog;
    private readonly PreferencesViewController _controller;

    [Gtk.Connect] private readonly Gtk.CheckButton _waveCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _levelsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _particlesCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _barsCheckButton;
    [Gtk.Connect] private readonly Gtk.CheckButton _spineCheckButton;
    [Gtk.Connect] private readonly Adw.ComboRow _mirrorRow;
    [Gtk.Connect] private readonly Adw.ActionRow _reverseMirrorRow;
    [Gtk.Connect] private readonly Gtk.Switch _reverseMirrorSwitch;
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
    [Gtk.Connect] private readonly Gtk.Button _addImageButton;
    [Gtk.Connect] private readonly Gtk.Scale _imageScale;
    [Gtk.Connect] private readonly Gtk.Stack _imagesStack;
    [Gtk.Connect] private readonly Gtk.FlowBox _imagesFlowBox;

    private PreferencesDialog(Gtk.Builder builder, PreferencesViewController controller, Adw.Application application) : base(builder.GetPointer("_root"), false)
    {
        _avoidCAVAReload = false;
        _removingImages = false;
        //Window Settings
        _controller = controller;
        SetIconName(_controller.ID);
        //Next Drawing Mode Action
        var actNextMode = Gio.SimpleAction.New("next-mode", null);
        actNextMode.OnActivate += (sender, e) =>
        {
            if (_controller.Mode < DrawingMode.SpineBox)
            {
                switch (_controller.Mode + 1)
                {
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
            }
            else
            {
                _waveCheckButton.SetActive(true);
            }
        };
        application.AddAction(actNextMode);
        application.SetAccelsForAction("app.next-mode", new string[] { "d" });
        //Previous Drawing Mode Action
        var actPrevMode = Gio.SimpleAction.New("prev-mode", null);
        actPrevMode.OnActivate += (sender, e) =>
        {
            if (_controller.Mode > DrawingMode.WaveBox)
            {
                switch (_controller.Mode - 1)
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
                }
            }
            else
            {
                _spineCheckButton.SetActive(true);
            }
        };
        application.AddAction(actPrevMode);
        application.SetAccelsForAction("app.prev-mode", new string[] { "<Shift>d" });
        //Next Mirror Mode Action
        var actNextMirror = Gio.SimpleAction.New("next-mirror", null);
        actNextMirror.OnActivate += (sender, e) =>
        {
            var maxMirror = _controller.Stereo ? Mirror.SplitChannels : Mirror.Full;
            _mirrorRow.SetSelected(_controller.Mirror < maxMirror ? (uint)_controller.Mirror + 1 : 0);
        };
        application.AddAction(actNextMirror);
        application.SetAccelsForAction("app.next-mirror", new string[] { "m" });
        //Previous Mirror Mode Action
        var actPrevMirror = Gio.SimpleAction.New("prev-mirror", null);
        actPrevMirror.OnActivate += (sender, e) =>
        {
            var maxMirror = _controller.Stereo ? Mirror.SplitChannels : Mirror.Full;
            _mirrorRow.SetSelected(_controller.Mirror > Mirror.Off ? (uint)_controller.Mirror - 1 : (uint)maxMirror);
        };
        application.AddAction(actPrevMirror);
        application.SetAccelsForAction("app.prev-mirror", new string[] { "<Shift>m" });
        //Toggle Reverse Mirror Action
        var actReverseMirror = Gio.SimpleAction.New("toggle-reverse-mirror", null);
        actReverseMirror.OnActivate += (sender, e) => _reverseMirrorSwitch.SetActive(!_reverseMirrorSwitch.GetActive());
        application.AddAction(actReverseMirror);
        application.SetAccelsForAction("app.toggle-reverse-mirror", new string[] { "v" });
        //Increase Area Margin Action
        var actIncMargin = Gio.SimpleAction.New("inc-margin", null);
        actIncMargin.OnActivate += (sender, e) =>
        {
            if (_marginScale.GetValue() < 40)
            {
                _marginScale.SetValue(_marginScale.GetValue() + 1);
            }
        };
        application.AddAction(actIncMargin);
        application.SetAccelsForAction("app.inc-margin", new string[] { "n" });
        //Decrease Area Margin Action
        var actDecMargin = Gio.SimpleAction.New("dec-margin", null);
        actDecMargin.OnActivate += (sender, e) =>
        {
            if (_marginScale.GetValue() > 0)
            {
                _marginScale.SetValue(_marginScale.GetValue() - 1);
            }
        };
        application.AddAction(actDecMargin);
        application.SetAccelsForAction("app.dec-margin", new string[] { "<Shift>n" });
        //Next Direction Action
        var actNextDir = Gio.SimpleAction.New("next-direction", null);
        actNextDir.OnActivate += (sender, e) => _directionRow.SetSelected(_controller.Direction < DrawingDirection.RightLeft ? (uint)_controller.Direction + 1 : (uint)DrawingDirection.TopBottom);
        application.AddAction(actNextDir);
        application.SetAccelsForAction("app.next-direction", new string[] { "o" });
        //Previous Direction Action
        var actPrevDir = Gio.SimpleAction.New("prev-direction", null);
        actPrevDir.OnActivate += (sender, e) => _directionRow.SetSelected(_controller.Direction > DrawingDirection.TopBottom ? (uint)_controller.Direction - 1 : (uint)DrawingDirection.RightLeft);
        application.AddAction(actPrevDir);
        application.SetAccelsForAction("app.prev-direction", new string[] { "<Shift>o" });
        //Increase Items Offset Action
        var actIncOffset = Gio.SimpleAction.New("inc-offset", null);
        actIncOffset.OnActivate += (sender, e) =>
        {
            if (_offsetScale.GetValue() < 20)
            {
                _offsetScale.SetValue(_offsetScale.GetValue() + 1);
            }
        };
        application.AddAction(actIncOffset);
        application.SetAccelsForAction("app.inc-offset", new string[] { "t" });
        //Decrease Items Offset Action
        var actDecOffset = Gio.SimpleAction.New("dec-offset", null);
        actDecOffset.OnActivate += (sender, e) =>
        {
            if (_offsetScale.GetValue() > 0)
            {
                _offsetScale.SetValue(_offsetScale.GetValue() - 1);
            }
        };
        application.AddAction(actDecOffset);
        application.SetAccelsForAction("app.dec-offset", new string[] { "<Shift>t" });
        //Increase Items Roundness Action
        var actIncRound = Gio.SimpleAction.New("inc-roundness", null);
        actIncRound.OnActivate += (sender, e) =>
        {
            if (_roundnessScale.GetValue() < 100)
            {
                _roundnessScale.SetValue(_roundnessScale.GetValue() + 1);
            }
        };
        application.AddAction(actIncRound);
        application.SetAccelsForAction("app.inc-roundness", new string[] { "r" });
        //Decrease Items Roundness Action
        var actDecRound = Gio.SimpleAction.New("dec-roundness", null);
        actDecRound.OnActivate += (sender, e) =>
        {
            if (_roundnessScale.GetValue() > 0)
            {
                _roundnessScale.SetValue(_roundnessScale.GetValue() - 1);
            }
        };
        application.AddAction(actDecRound);
        application.SetAccelsForAction("app.dec-roundness", new string[] { "<Shift>r" });
        //Toggle Filling Action
        var actFill = Gio.SimpleAction.New("toggle-filling", null);
        actFill.OnActivate += (sender, e) => _fillingSwitch.SetActive(!_fillingSwitch.GetActive());
        application.AddAction(actFill);
        application.SetAccelsForAction("app.toggle-filling", new string[] { "f" });
        //Increase Lines Thickness Action
        var actIncThick = Gio.SimpleAction.New("inc-thickness", null);
        actIncThick.OnActivate += (sender, e) =>
        {
            if (_thicknessScale.GetValue() < 10)
            {
                _thicknessScale.SetValue(_thicknessScale.GetValue() + 1);
            }
        };
        application.AddAction(actIncThick);
        application.SetAccelsForAction("app.inc-thickness", new string[] { "l" });
        //Decrease Lines Thickness Action
        var actDecThick = Gio.SimpleAction.New("dec-thickness", null);
        actDecThick.OnActivate += (sender, e) =>
        {
            if (_thicknessScale.GetValue() > 0)
            {
                _thicknessScale.SetValue(_thicknessScale.GetValue() - 1);
            }
        };
        application.AddAction(actDecThick);
        application.SetAccelsForAction("app.dec-thickness", new string[] { "<Shift>l" });
        //Toggle Window Borders Action
        var actBorders = Gio.SimpleAction.New("toggle-borders", null);
        actBorders.OnActivate += (sender, e) => _borderlessSwitch.SetActive(!_borderlessSwitch.GetActive());
        application.AddAction(actBorders);
        application.SetAccelsForAction("app.toggle-borders", new string[] { "W" });
        //Toggle Sharp Corners Action
        var actCorners = Gio.SimpleAction.New("toggle-corners", null);
        actCorners.OnActivate += (sender, e) => _sharpCornersSwitch.SetActive(!_sharpCornersSwitch.GetActive());
        application.AddAction(actCorners);
        application.SetAccelsForAction("app.toggle-corners", new string[] { "S" });
        //Increase Bars Action
        var actIncBars = Gio.SimpleAction.New("inc-bars", null);
        actIncBars.OnActivate += (sender, e) =>
        {
            if (_controller.BarPairs < 25)
            {
                _barsScale.SetValue((_controller.BarPairs + 1) * 2);
            }
        };
        application.AddAction(actIncBars);
        application.SetAccelsForAction("app.inc-bars", new string[] { "b" });
        //Decrease Bars Action
        var actDecBars = Gio.SimpleAction.New("dec-bars", null);
        actDecBars.OnActivate += (sender, e) =>
        {
            if (_controller.BarPairs > 3)
            {
                _barsScale.SetValue((_controller.BarPairs - 1) * 2);
            }
        };
        application.AddAction(actDecBars);
        application.SetAccelsForAction("app.dec-bars", new string[] { "<Shift>b" });
        //Toggle Stereo Action
        var actStereo = Gio.SimpleAction.New("toggle-stereo", null);
        actStereo.OnActivate += (sender, e) => _stereoButton.SetActive(!_stereoButton.GetActive());
        application.AddAction(actStereo);
        application.SetAccelsForAction("app.toggle-stereo", new string[] { "c" });
        //Toggle Reverse Order Action
        var actReverse = Gio.SimpleAction.New("toggle-reverse", null);
        actReverse.OnActivate += (sender, e) => _reverseSwitch.SetActive(!_reverseSwitch.GetActive());
        application.AddAction(actReverse);
        application.SetAccelsForAction("app.toggle-reverse", new string[] { "e" });
        //Next Color Profile Action
        var actNextProfile = Gio.SimpleAction.New("next-profile", null);
        actNextProfile.OnActivate += (sender, e) =>
        {
            if (_controller.ActiveProfile < _controller.ColorProfiles.Count - 1)
            {
                _profilesList.SelectRow(_profilesList.GetRowAtIndex(_controller.ActiveProfile + 1));
            }
            else
            {
                _profilesList.SelectRow(_profilesList.GetRowAtIndex(0));
            }
        };
        application.AddAction(actNextProfile);
        application.SetAccelsForAction("app.next-profile", new string[] { "p" });
        //Previous Color Profile Action
        var actPrevProfile = Gio.SimpleAction.New("prev-profile", null);
        actPrevProfile.OnActivate += (sender, e) =>
        {
            if (_controller.ActiveProfile > 0)
            {
                _profilesList.SelectRow(_profilesList.GetRowAtIndex(_controller.ActiveProfile - 1));
            }
            else
            {
                _profilesList.SelectRow(_profilesList.GetRowAtIndex(_controller.ColorProfiles.Count - 1));
            }
        };
        application.AddAction(actPrevProfile);
        application.SetAccelsForAction("app.prev-profile", new string[] { "<Shift>p" });
        //Build UI
        builder.Connect(this);
        OnCloseRequest += (sender, e) =>
        {
            _controller.Save();
            return false;
        };
        LoadInstantSettings();
        LoadCAVASettings();
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
        _offsetRow.SetSensitive(_controller.Mode != DrawingMode.WaveBox);
        _roundnessRow.SetSensitive(_controller.Mode != DrawingMode.WaveBox && _controller.Mode != DrawingMode.BarsBox);
        _mirrorRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Mirror = (Mirror)_mirrorRow.GetSelected();
                _reverseMirrorRow.SetVisible(_controller.Mirror != Mirror.Off);
            }
        };
        _reverseMirrorSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ReverseMirror = _reverseMirrorSwitch.GetActive();
            }
        };
        _marginScale.OnValueChanged += (sender, e) =>
        {
            _controller.AreaMargin = (uint)_marginScale.GetValue();
        };
        _directionRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Direction = (DrawingDirection)_directionRow.GetSelected();
            }
        };
        _offsetScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsOffset = (float)_offsetScale.GetValue() / 100.0f;
        };
        _roundnessScale.OnValueChanged += (sender, e) =>
        {
            _controller.ItemsRoundness = (float)_roundnessScale.GetValue() / 100.0f;
        };
        _fillingSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Filling = _fillingSwitch.GetActive();
            }
        };
        _thicknessScale.OnValueChanged += (sender, e) =>
        {
            _controller.LinesThickness = (float)_thicknessScale.GetValue();
        };
        _thicknessRow.SetSensitive(!_controller.Filling);
        _borderlessSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Borderless = _borderlessSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _sharpCornersSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.SharpCorners = _sharpCornersSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _windowControlsSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.ShowControls = _windowControlsSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _autohideHeaderSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.AutohideHeader = _autohideHeaderSwitch.GetActive();
                _controller.ChangeWindowSettings();
            }
        };
        _framerateRow.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "selected")
            {
                _controller.Framerate = new []{ 30u, 60u, 90u, 120u, 144u }[_framerateRow.GetSelected()];
                if (!_avoidCAVAReload)
                {
                    _controller.ChangeCAVASettings();
                }
            }
        };
        _barsScale.OnValueChanged += (sender, e) =>
        {
            if (_barsScale.GetValue() % 2 != 0)
            {
                _barsScale.SetValue(_barsScale.GetValue() - 1);
                return;
            }
            _controller.BarPairs = (uint)(_barsScale.GetValue() / 2);
            if (!_avoidCAVAReload)
            {
                _controller.ChangeCAVASettings();
            }
        };
        _autosensSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Autosens = _autosensSwitch.GetActive();
                if (!_avoidCAVAReload)
                {
                    _controller.ChangeCAVASettings();
                }
            }
        };
        _sensitivityScale.OnValueChanged += (sender, e) =>
        {
            _controller.Sensitivity = (uint)_sensitivityScale.GetValue();
            if (!_avoidCAVAReload)
            {
                _controller.ChangeCAVASettings();
            }
        };
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
            if (!_avoidCAVAReload)
            {
                _controller.ChangeCAVASettings();
            }
        };
        _monstercatSwitch.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "active")
            {
                _controller.Monstercat = _monstercatSwitch.GetActive();
                if (!_avoidCAVAReload)
                {
                    _controller.ChangeCAVASettings();
                }
            }
        };
        _noiseReductionScale.GetFirstChild().SetMarginBottom(12);
        _noiseReductionScale.AddMark(0.77, Gtk.PositionType.Bottom, null);
        _noiseReductionScale.OnValueChanged += (sender, e) =>
        {
            _controller.NoiseReduction = (float)_noiseReductionScale.GetValue();
            if (!_avoidCAVAReload)
            {
                _controller.ChangeCAVASettings();
            }
        };
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
                UpdateColorsGrid();
            }
        };
        _addProfileButton.OnClicked += OnAddProfile;
        _lightThemeButton.OnToggled += (sender, e) =>
        {
            _controller.ColorProfiles[_controller.ActiveProfile].Theme = _lightThemeButton.GetActive() ? Theme.Light : Theme.Dark;
            _controller.ChangeWindowSettings();
        };
        _colorDialog = Gtk.ColorDialog.New();
        _addFgColorButton.OnClicked += (sender, e) => AddColorAsync(ColorType.Foreground);
        _addBgColorButton.OnClicked += (sender, e) => AddColorAsync(ColorType.Background);
        UpdateColorsGrid();
        _addImageButton.OnClicked += async (sender, e) => await AddImageAsync();
        _imageScale.OnValueChanged += (sender, e) => _controller.ImageScale = (float)_imageScale.GetValue();
        _imagesFlowBox.OnSelectedChildrenChanged += (sender, e) =>
        {
            if (!_removingImages)
            {
                _controller.ImageIndex = _imagesFlowBox.GetSelectedChildrenIndices()[0] - 1;
            }
        };
        // Update view when controller has changed by cmd options
        _controller.OnUpdateViewInstant += () => GLib.Functions.IdleAdd(0, LoadInstantSettings);
        _controller.OnUpdateViewCAVA += () => GLib.Functions.IdleAdd(0, LoadCAVASettings);
    }

    /// <summary>
    /// Load settings that don't require CAVA restart
    /// </summary>
    public bool LoadInstantSettings()
    {
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
        var mirror = (uint)_controller.Mirror; // saving mirror state to apply after changing the model
        if (_controller.Stereo)
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("Full"), _("Split Channels") }));
            _mirrorRow.SetSelected(mirror);
        }
        else
        {
            _mirrorRow.SetModel(Gtk.StringList.New(new string[] { _("Off"), _("On") }));
            if (mirror == (uint)Mirror.SplitChannels)
            {
                _mirrorRow.SetSelected(1u);
            }
            else
            {
                _mirrorRow.SetSelected(mirror);
            }
        }
        _reverseMirrorRow.SetVisible(_controller.Mirror != Mirror.Off);
        _reverseMirrorSwitch.SetActive(_controller.ReverseMirror);
        _marginScale.SetValue((int)_controller.AreaMargin);
        _directionRow.SetSelected((uint)_controller.Direction);
        _offsetScale.SetValue((int)(_controller.ItemsOffset * 100));
        _roundnessScale.SetValue((int)(_controller.ItemsRoundness * 100));
        _thicknessRow.SetSensitive(!_fillingSwitch.GetActive());
        _fillingSwitch.SetActive(_controller.Filling);
        _thicknessScale.SetValue((int)_controller.LinesThickness);
        _borderlessSwitch.SetActive(_controller.Borderless);
        _sharpCornersSwitch.SetActive(_controller.SharpCorners);
        _windowControlsSwitch.SetActive(_controller.ShowControls);
        _autohideHeaderSwitch.SetActive(_controller.AutohideHeader);
        _reverseSwitch.SetActive(_controller.ReverseOrder);
        UpdateColorProfiles();
        UpdateImagesList();
        _imageScale.SetValue(_controller.ImageScale);
        return false;
    }

    /// <summary>
    /// Load settings that require CAVA restart
    /// </summary>
    public bool LoadCAVASettings()
    {
        _avoidCAVAReload = true;
        _framerateRow.SetSelected(_controller.Framerate / 30u - 1u);
        _barsScale.SetValue((int)_controller.BarPairs * 2);
        _autosensSwitch.SetActive(_controller.Autosens);
        _sensitivityScale.SetValue((int)_controller.Sensitivity);
        _stereoButton.SetActive(_controller.Stereo);
        _monstercatSwitch.SetActive(_controller.Monstercat);
        _noiseReductionScale.SetValue(_controller.NoiseReduction);
        _avoidCAVAReload = false;
        _controller.ChangeCAVASettings();
        return false;
    }

    /// <summary>
    /// Constructs a PreferencesDialog
    /// </summary>
    /// <param name="controller">PreferencesViewController</param>
    public PreferencesDialog(PreferencesViewController controller, Adw.Application application) : this(Builder.FromFile("preferences_dialog.ui"), controller, application)
    {
    }

    /// <summary>
    /// Reload color profiles
    /// </summary>
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

    /// <summary>
    /// Occurs when add profile button was clicked
    /// </summary>
    /// <param name="sender">Button</param>
    /// <param name="e">EventArgs</param>
    private void OnAddProfile(object sender, EventArgs e)
    {
        var dialog = new AddProfileDialog(this, _controller.ID);
        dialog.OnResponse += (sender, e) =>
        {
            if (e.Response == "suggested")
            {
                _controller.AddColorProfile(dialog.ProfileName);
                UpdateColorProfiles();
            }
        };
        dialog.Present();
    }

    /// <summary>
    /// Occurs when delete profile button was clicked
    /// </summary>
    /// <param name="sender">Profile box of profile that should be deleted</param>
    /// <param name="index">Profile index</param>
    private void OnDeleteProfile(object sender, int index)
    {
        var dialog = new MessageDialog(
            this, _controller.ID,
            _("Delete Profile"), _("Are you sure you want to delete profile \"{0}\"?", _controller.ColorProfiles[index].Name),
            _("Cancel"), _("Delete"));
        dialog.OnResponse += (sender, e) =>
        {
            if (e.Response == "destructive")
            {
                _controller.ActiveProfile -= 1;
                _controller.ColorProfiles.RemoveAt(index);
                UpdateColorProfiles();
            }
        };
        dialog.Present();
    }

    /// <summary>
    /// Reload colors grid
    /// </summary>
    private void UpdateColorsGrid()
    {
        _lightThemeButton.SetActive(_controller.ColorProfiles[_controller.ActiveProfile].Theme == Theme.Light);
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

    /// <summary>
    /// Add color to grid
    /// </summary>
    /// <param name="type">Color type (background or foreground)</param>
    private async Task AddColorAsync(ColorType type)
    {
        try
        {
            var color = await _colorDialog.ChooseRgbaAsync(this);
            var alpha = color?.Alpha ?? 0;
            var red = color?.Red ?? 0;
            var green = color?.Green ?? 0;
            var blue = color?.Blue ?? 0;
            if (alpha <= 0.0001f)
            {
                red = 0;
                green = 0;
                blue = 0;
            }
            _controller.AddColor(type, $"#{((int)(alpha * 255)).ToString("x2")}{((int)(red * 255)).ToString("x2")}{((int)(green * 255)).ToString("x2")}{((int)(blue * 255)).ToString("x2")}");
            UpdateColorsGrid();
        }
        catch { }
    }

    /// <summary>
    /// Occurs when color button was clicked
    /// </summary>
    /// <param name="sender">Color box that should be edited</param>
    /// <param name="e">Color args for controller</param>
    private void OnEditColor(object sender, ColorEventArgs e) => _controller.EditColor(e.Type, e.Index, e.Color);

    /// <summary>
    /// Occurs when delete color button was clicked
    /// </summary>
    /// <param name="sender">Color box that should be deleted</param>
    /// <param name="e">Color args for controller</param>
    private void OnDeleteColor(object sender, ColorEventArgs e)
    {
        _controller.DeleteColor(e.Type, e.Index);
        UpdateColorsGrid();
    }

    /// <summary>
    /// Update flowbox with images
    /// </summary>
    public void UpdateImagesList()
    {
        var paths = _controller.ImagesList;
        if (paths.Count == 0)
        {
            _imagesStack.SetVisibleChildName("empty");
            _imageScale.SetSensitive(false);
            return;
        }
        _imageScale.SetSensitive(true);
        _imagesStack.SetVisibleChildName("images");
        _removingImages = true;
        while (_imagesFlowBox.GetFirstChild() != null)
        {
            _imagesFlowBox.Remove(_imagesFlowBox.GetFirstChild()!);
        }
        _removingImages = false;
        var emptyImage = Gtk.Image.NewFromIconName("x-circular-symbolic");
        emptyImage.SetTooltipText(_("No Image"));
        emptyImage.AddCssClass("cavalier-image");
        emptyImage.SetSizeRequest(160, 120);
        emptyImage.SetPixelSize(42);
        emptyImage.SetMarginTop(2);
        emptyImage.SetMarginStart(2);
        emptyImage.SetMarginEnd(2);
        emptyImage.SetMarginBottom(2);
        _imagesFlowBox.Append(emptyImage);
        for (var i = 0; i < paths.Count; i++)
        {
            var image = new ImageItem(paths[i], i);
            image.OnRemoveImage += RemoveImage;
            _imagesFlowBox.Append(image);
        }
        try
        {
            _imagesFlowBox.SelectChild(_imagesFlowBox.GetChildAtIndex(_controller.ImageIndex + 1)!);
        }
        catch (IndexOutOfRangeException)
        {
            _imagesFlowBox.SelectChild(_imagesFlowBox.GetChildAtIndex(0)!);
        }
        _imageScale.SetValue(_controller.ImageScale);
    }

    /// <summary>
    /// Add an image to the images list
    /// </summary>
    public async Task AddImageAsync()
    {
        var dialog = Gtk.FileDialog.New();
        dialog.SetTitle(_("Select an image"));
        dialog.SetAcceptLabel(_("Add"));
        var filter = Gtk.FileFilter.New();
        filter.SetName(_("JPEG and PNG images"));
        filter.AddPattern("*.jpg");
        filter.AddPattern("*.jpeg");
        filter.AddPattern("*.png");
        var filters = Gio.ListStore.New(Gtk.FileFilter.GetGType());
        filters.Append(filter);
        dialog.SetFilters(filters);
        dialog.SetDefaultFilter(filter);
        try
        {
            var file = await dialog.OpenAsync(this);
            _controller.AddImage(file.GetPath());
            UpdateImagesList();
        }
        catch { }
    }

    /// <summary>
    /// Remove an image from the images list
    /// </summary>
    /// <param name="index">Index of image to remove</param>
    public void RemoveImage(int index)
    {
        var paths = _controller.ImagesList;
        File.Delete(paths[index]);
        UpdateImagesList();
    }
}
