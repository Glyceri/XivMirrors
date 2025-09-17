using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct MyDevice
{
    [FieldOffset(0x920)] public Texture* someTexture;
}
