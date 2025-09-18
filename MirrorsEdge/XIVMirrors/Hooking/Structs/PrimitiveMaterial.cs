using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

[StructLayout(LayoutKind.Sequential, Size = 24)]
public struct PrimitiveMaterial
{
    public BlendState BlendState;
    public int Unknown;
    public nint Texture;
    public SamplerState SamplerState;
    public PrimitiveMaterialParams Params;
}
