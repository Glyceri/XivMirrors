using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Resources.Struct;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ScaledResolution
{
    public readonly uint AssignedWidth;
    public readonly uint AssignedHeight;
    public readonly uint ActualWidth;
    public readonly uint ActualHeight;

    public ScaledResolution(uint assignedWidth, uint assignedHeight, uint actualWidth, uint actualHeight)
    {
        AssignedWidth   = assignedWidth;
        AssignedHeight  = assignedHeight;
        ActualWidth     = actualWidth;
        ActualHeight    = actualHeight;
    }
}
