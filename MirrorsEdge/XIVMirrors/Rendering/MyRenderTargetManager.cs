using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit, Size = 1840)]
internal unsafe struct MyRenderTargetManager
{
    [FieldOffset(0x20)] public Texture* DepthBuffer; // Depth/Stencil?
}
