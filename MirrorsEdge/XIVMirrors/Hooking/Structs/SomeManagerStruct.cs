using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

[StructLayout(LayoutKind.Explicit, Size = 0x48C7)]
internal unsafe struct SomeManagerStruct
{
    [FieldOffset(0x4040)] public Texture* DepthStencil;
}
