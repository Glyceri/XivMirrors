using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using System;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class ShaderHandler : IDisposable
{
    private readonly MirrorServices MirrorServices;
    private readonly ResourceLoader ResourceLoader;
    private readonly DirectXData    DirectXData;

    public readonly  Shader         UIShader;
    public readonly  Shader         MirrorShader;

    public readonly ShaderFactory Factory;

    public ShaderHandler(MirrorServices mirrorServices, ResourceLoader resourceLoader, DirectXData directXData)
    {
        MirrorServices  = mirrorServices;
        ResourceLoader  = resourceLoader;
        DirectXData     = directXData;

        Factory         = new ShaderFactory(MirrorServices, ResourceLoader, DirectXData);

        UIShader        = new Shader(Factory, "VertexShader.hlsl", "FragmentShader.hlsl", []);
        MirrorShader    = new Shader(Factory, "MirrorVertexShader.hlsl", "MirrorFragmentShader.hlsl", []);
    }

    public void Dispose()
    {
        UIShader.Dispose();
        MirrorShader.Dispose();
    }
}
