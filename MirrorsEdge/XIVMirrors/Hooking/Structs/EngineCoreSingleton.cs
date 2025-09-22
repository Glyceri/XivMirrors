using FFXIVClientStructs.FFXIV.Common.Math;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct EngineCoreSingleton
{
    [FieldOffset(0x1B4)] public Matrix4x4* ViewProjectionMatrix;
    [FieldOffset(0x174)] public Matrix4x4* ProjectionMatrix;
    [FieldOffset(0X134)] public Matrix4x4* ViewMatrix;
}
