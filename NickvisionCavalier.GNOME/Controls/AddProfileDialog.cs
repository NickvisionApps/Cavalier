using System;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// A dialog for showing a message
/// </summary>
public class AddProfileDialog
{
    private readonly Adw.MessageDialog _dialog;

    /// <summary>
    /// New profile name
    /// </summary>
    public string ProfileName { get; set; }
    
    /// <summary>
    /// Constructs a MessageDialog
    /// </summary>
    /// <param name="parentWindow">Gtk.Window</param>
    /// <param name="iconName">The name of the icon of the dialog</param>
    public AddProfileDialog(Gtk.Window parentWindow, string iconName)
    {
        ProfileName = "";
        _dialog = Adw.MessageDialog.New(parentWindow, _("Add New Profile"), _("New color profile will be a copy of the current active profile."));
        _dialog.SetIconName(iconName);
        _dialog.SetSizeRequest(360, -1);
        var prefGroup = Adw.PreferencesGroup.New();
        _dialog.SetExtraChild(prefGroup);
        var nameEntry = Adw.EntryRow.New();
        nameEntry.SetTitle(_("Profile Name"));
        nameEntry.OnNotify += (sender, e) =>
        {
            if (e.Pspec.GetName() == "text")
            {
                _dialog.SetResponseEnabled("suggested", nameEntry.GetText().Length > 0);
                ProfileName = nameEntry.GetText();
            }
        };
        prefGroup.Add(nameEntry);
        _dialog.AddResponse("cancel", _("Cancel"));
        _dialog.SetDefaultResponse("cancel");
        _dialog.SetCloseResponse("cancel");
        _dialog.AddResponse("suggested", _("Add"));
        _dialog.SetResponseAppearance("suggested", Adw.ResponseAppearance.Suggested);
        _dialog.SetResponseEnabled("suggested", false);
    }
    
    public event GObject.SignalHandler<Adw.MessageDialog, Adw.MessageDialog.ResponseSignalArgs> OnResponse
    {
        add
        {
            _dialog.OnResponse += value;
        }
        remove
        {
            _dialog.OnResponse -= value;
        }
    }
    
    /// <summary>
    /// Presents the dialog
    /// </summary>
    public void Present() => _dialog.Present();
}