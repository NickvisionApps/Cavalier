using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.GNOME.Controls;

/// <summary>
/// A dialog with command-line options help
/// </summary>
public class CommandHelpDialog
{
    private readonly Adw.MessageDialog _dialog;
    
    /// <summary>
    /// Construct CommandHelpDialog
    /// </summary>
    public CommandHelpDialog(Gtk.Window parent, string help)
    {
        _dialog = Adw.MessageDialog.New(parent, _("Command Help"), "");
        var label = Gtk.Label.New(help);
        label.SetMarginTop(6);
        label.SetMarginStart(6);
        label.SetMarginEnd(6);
        label.SetMarginBottom(6);
        var scrolledWindow = Gtk.ScrolledWindow.New();
        scrolledWindow.SetChild(label);
        scrolledWindow.AddCssClass("card");
        scrolledWindow.SetPolicy(Gtk.PolicyType.Never, Gtk.PolicyType.Automatic);
        scrolledWindow.SetMinContentHeight(300);
        _dialog.SetExtraChild(scrolledWindow);
        _dialog.AddResponse("close", _("Close"));
        _dialog.SetCloseResponse("close");
    }
    
    /// <summary>
    /// Present the dialog
    /// </summary>
    public void Present() => _dialog.Present();
}