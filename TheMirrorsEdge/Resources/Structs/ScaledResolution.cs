using System.Runtime.InteropServices;

namespace TheMirrorsEdge.Resources.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 16)]
public readonly struct ScaledResolution(int assignedWidth, int assignedHeight, int actualWidth, int actualHeight)
{
    public readonly int AssignedWidth  = assignedWidth;
    public readonly int AssignedHeight = assignedHeight;
    public readonly int ActualWidth    = actualWidth;
    public readonly int ActualHeight   = actualHeight;
}