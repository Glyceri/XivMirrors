using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

[StructLayout(LayoutKind.Sequential, Size = 4)]
internal struct InputElement
{
    public byte Slot;
    public byte Offset;
    public byte Format;
    public byte Semantic;
}
