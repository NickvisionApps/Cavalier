using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NickvisionCavalier.GNOME.Helpers;

/// <summary>
/// Helper methods for GTK
/// </summary>
public unsafe static partial class GtkHelpers
{
    [StructLayout(LayoutKind.Sequential)]
    private struct GLibList
    {
        public nint data;
        public GLibList* next;
        public GLibList* prev;
    }

    [LibraryImport("libadwaita-1.so.0", StringMarshalling = StringMarshalling.Utf8)]
    private static partial void gtk_color_dialog_button_set_rgba(nint button, ref GdkHelpers.RGBA rgba);
    [LibraryImport("libadwaita-1.so.0")]
    private static partial GLibList* gtk_flow_box_get_selected_children(nint box);
    [LibraryImport("libadwaita-1.so.0")]
    private static partial int gtk_flow_box_child_get_index(nint row);

    /// <summary>
    /// Helper extension method for Gtk.ColorButton to get color as GdkHelpers.RGBA
    /// </summary>
    /// <param name="button">Color button</param>
    /// <returns>Color as GdkHelpers.RGBA</returns>
    public static GdkHelpers.RGBA GetExtRgba(this Gtk.ColorDialogButton button) => Marshal.PtrToStructure<GdkHelpers.RGBA>(button.GetRgba().Handle.DangerousGetHandle());

    /// <summary>
    /// Helper extension method for Gtk.ColorButton to set color as GdkHelpers.RGBA
    /// </summary>
    /// <param name="button">Color button</param>
    /// <param name="color">Color as GdkHelpers.RGBA</param>
    public static void SetExtRgba(this Gtk.ColorDialogButton button, GdkHelpers.RGBA color) => gtk_color_dialog_button_set_rgba(button.Handle, ref color);

    /// <summary>
    /// Extension method for Gtk.ColorDialog to choose a color
    /// </summary>
    /// <param name="dialog">Color dialog</param>
    /// <param name="parent">Parent window</param>
    /// <exception cref="Exception">Thrown if failed to choose a color</exception>
    /// <returns>Color struct if successful, or null</returns>
    public static Task<GdkHelpers.RGBA?> ChooseRgbaAsync(this Gtk.ColorDialog dialog, Gtk.Window parent)
    {
        var tcs = new TaskCompletionSource<GdkHelpers.RGBA?>();

        var callback = new Gio.Internal.AsyncReadyCallbackAsyncHandler((sourceObject, res, data) =>
        {
            if (sourceObject is null)
            {
                tcs.SetException(new Exception("Missing source object"));
            }
            else
            {
                var color = Gtk.Internal.ColorDialog.ChooseRgbaFinish(sourceObject.Handle, res.Handle, out var error);
                if (!error.IsInvalid)
                {
                    tcs.SetException(new Exception(error.ToString() ?? ""));
                }
                else if (color.DangerousGetHandle() == IntPtr.Zero)
                {
                    tcs.SetResult(null);
                }
                else
                {
                    tcs.SetResult(Marshal.PtrToStructure<GdkHelpers.RGBA>(color.DangerousGetHandle()));
                }
            }
        });

        Gtk.Internal.ColorDialog.ChooseRgba(
            self: dialog.Handle,
            parent: parent.Handle,
            initialColor: new Gdk.Internal.RGBAOwnedHandle(IntPtr.Zero),
            cancellable: IntPtr.Zero,
            callback: callback.NativeCallback,
            userData: IntPtr.Zero
            );

        return tcs.Task;
    }

    /// <summary>
    /// Helper extension method for Gtk.FlowBox to get indices of selected children
    /// </summary>
    /// <param name="box">Flow box</param>
    /// <returns>List of indices</returns>
    public static List<int> GetSelectedChildrenIndices(this Gtk.FlowBox box)
    {
        var list = new List<int>();
        var firstSelectedRowPtr = gtk_flow_box_get_selected_children(box.Handle);
        for (var ptr = firstSelectedRowPtr; ptr != null; ptr = ptr->next)
        {
            list.Add(gtk_flow_box_child_get_index(ptr->data));
        }
        return list;
    }
}
