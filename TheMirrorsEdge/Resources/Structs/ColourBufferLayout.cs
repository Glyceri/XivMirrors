using System.Runtime.InteropServices;
using SharpDX;

namespace TheMirrorsEdge.Resources.Structs;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct ColourBufferLayout(Vector4 colour)
{
    public readonly Vector4 Colour = colour;
}