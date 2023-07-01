using System.Runtime.InteropServices;

namespace NickvisionCavalier.GNOME.Helpers;

[StructLayout(LayoutKind.Sequential)]
public struct Color
{
    public float Red;
    public float Green;
    public float Blue;
    public float Alpha;
}