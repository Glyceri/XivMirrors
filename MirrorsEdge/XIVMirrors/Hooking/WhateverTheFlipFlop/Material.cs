using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Memory;
using System;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.WhateverTheFlipFlop;

internal unsafe class Material : IDisposable
{
    public nint Pointer { get; }

    private PrimitiveMaterial* PrimitiveMaterial => (PrimitiveMaterial*)Pointer;

    private Material()
    {
        Pointer = Marshal.AllocHGlobal(Marshal.SizeOf<PrimitiveMaterial>());
    }

    public static Material CreateFromTexture(nint texture, DirectXData data)
    {
        var material = new Material();

        material.PrimitiveMaterial->BlendState = new Structs.BlendState
        {
            ColorWriteEnable = ColorMask.RGB,
            AlphaBlendFactorDst = 0x5,
            AlphaBlendFactorSrc = 0x0,
            AlphaBlendOperation = 0,
            ColorBlendFactorDst = 0x5,
            ColorBlendFactorSrc = 0x4,
            ColorBlendOperation = 0,
            Enable = true,
        };

        material.PrimitiveMaterial->Texture = texture;

        material.PrimitiveMaterial->SamplerState = new Structs.SamplerState
        {
            GammaEnable = false,
            MaxAnisotropy = 0,
            MinLOD = 0x0,
            MipLODBias = 0,
            Filter = 9,
            AddressW = 0,
            AddressV = 0,
            AddressU = 0,
        };

        material.PrimitiveMaterial->Params = new PrimitiveMaterialParams
        {
            FaceCullMode = 0,
            FaceCullEnable = false,
            DepthWriteEnable = false,
            DepthTestEnable = false,
            TextureRemapAlpha = 1,
            TextureRemapColor = 2,
        };

        return material;
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(Pointer);
        GC.SuppressFinalize(this);
    }
}
