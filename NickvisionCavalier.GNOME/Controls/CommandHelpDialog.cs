using NickvisionCavalier.GNOME.Helpers;
using static Nickvision.Aura.Localization.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// A dialog to show command-line help
/// </summary>
public class CommandHelpDialog : Adw.Window
{
    [Gtk.Connect] private readonly Gtk.Label _helpLabel;

    /// <summary>
    /// Construct a CommandHelpDialog
    /// </summary>
    /// <param name="builder">Gtk.Builder</param>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="help">Help text</param>
    private CommandHelpDialog(Gtk.Builder builder, Gtk.Window parent, string iconName, string help) : base(builder.GetPointer("_root"), false)
    {
        builder.Connect(this);
        //Dialog Settings
        SetIconName(iconName);
        SetTransientFor(parent);
        _helpLabel.SetLabel(help);
    }

    /// <summary>
    /// Constructs a HistoryDialog
    /// </summary>
    /// <param name="parent">Gtk.Window</param>
    /// <param name="iconName">Icon name for the window</param>
    /// <param name="help">Help text</param>
    public CommandHelpDialog(Gtk.Window parent, string iconName, string help) : this(Builder.FromFile("command_help_dialog.ui"), parent, iconName, help)
    {
    }
}