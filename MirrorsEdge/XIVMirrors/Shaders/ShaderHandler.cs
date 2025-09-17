using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class ShaderHandler : IDisposable
{
    private readonly MirrorServices MirrorServices;
    private readonly ResourceLoader ResourceLoader;
    private readonly DirectXData    DirectXData;

    public readonly  Shader         AlphaShader;
    public readonly  Shader         ClippedShader;
    public readonly  Shader         MirrorShader;

    public readonly ShaderFactory Factory;

    public ShaderHandler(MirrorServices mirrorServices, ResourceLoader resourceLoader, DirectXData directXData)
    {
        MirrorServices  = mirrorServices;
        ResourceLoader  = resourceLoader;
        DirectXData     = directXData;

        Factory         = new ShaderFactory(MirrorServices, ResourceLoader, DirectXData);

        AlphaShader     = new Shader(Factory, "AlphaVertexShader.hlsl",     "AlphaFragmentShader.hlsl",     []);
        ClippedShader   = new Shader(Factory, "ClippedVertexShader.hlsl",   "ClippedFragmentShader.hlsl",   [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)]);
        MirrorShader    = new Shader(Factory, "MirrorVertexShader.hlsl",    "MirrorFragmentShader.hlsl",    []);
    }

    public void Dispose()
    {
        AlphaShader?.Dispose();
        ClippedShader?.Dispose();
        MirrorShader?.Dispose();
    }
}
