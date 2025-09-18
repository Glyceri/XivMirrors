using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct TextureResourceHandle
{
    [FieldOffset(0x0)] public ResourceHandle Handle;

    [FieldOffset(0x0)] public FFXIVClientStructs.FFXIV.Client.System.Resource.Handle.TextureResourceHandle CsHandle;

    [FieldOffset(0x104)] public byte SomeLodFlag;

    public readonly bool ChangeLod
        => (SomeLodFlag & 1) != 0;
}
