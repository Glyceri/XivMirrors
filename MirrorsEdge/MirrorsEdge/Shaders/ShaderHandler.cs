using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.MirrorsEdge.Resources;
using MirrorsEdge.MirrorsEdge.Services;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.MirrorsEdge.Shaders;

internal class ShaderHandler : IDisposable
{
    private readonly MirrorServices MirrorServices;
    private readonly ResourceLoader ResourceLoader;
    private readonly DirectXData    DirectXData;

    public readonly VertexShader VertexShader;
    public readonly PixelShader  FragmentShader;
    public readonly InputLayout  InputLayout;
    public readonly SamplerState SamplerState;

    public readonly ShaderFactory Factory;

    public ShaderHandler(MirrorServices mirrorServices, ResourceLoader resourceLoader, DirectXData directXData)
    {
        MirrorServices  = mirrorServices;
        ResourceLoader  = resourceLoader;
        DirectXData     = directXData;

        Factory = new ShaderFactory(MirrorServices, ResourceLoader, DirectXData);

        bool failure = false;

        failure |= !Factory.GetVertexShader("VertexShader.hlsl", [], out VertexShader!, out InputLayout!, out SamplerState!);
        failure |= !Factory.GetFragmentShader("FragmentShader.hlsl", out FragmentShader!);

        if (failure)
        {
            throw new Exception("Shaders failed to initialize");
        }


    }

    public void Dispose()
    {
        InputLayout?.Dispose();
        SamplerState?.Dispose();
        FragmentShader?.Dispose();
        VertexShader?.Dispose();
    }
}
