using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.shaders.ShaderTypes;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class ShaderHandler : IDisposable
{
    private readonly MirrorServices         MirrorServices;
    private readonly ResourceLoader         ResourceLoader;
    private readonly DirectXData            DirectXData;

    public readonly  ImageMappedShader      AlphaShader;
    public readonly  ImageMappedShader      ClippedShader;
    public readonly  ImageMappedShader      InvertAlphaShader;
    public readonly  TransparentMimicShader TransparentMimicShader;
    public readonly  MirrorShader           MirrorShader;
    public readonly  Shader                 ShadedModelShader;

    public readonly ShaderFactory Factory;

    public ShaderHandler(MirrorServices mirrorServices, ResourceLoader resourceLoader, DirectXData directXData)
    {
        MirrorServices          = mirrorServices;
        ResourceLoader          = resourceLoader;
        DirectXData             = directXData;

        Factory                 = new ShaderFactory(MirrorServices, ResourceLoader, DirectXData);

        AlphaShader             = new ImageMappedShader     (DirectXData, MirrorServices, Factory, "AlphaFragmentShader.hlsl");
        ClippedShader           = new ImageMappedShader     (DirectXData, MirrorServices, Factory, "ClippedFragmentShader.hlsl");
        InvertAlphaShader       = new ImageMappedShader     (DirectXData, MirrorServices, Factory, "InvertAlphaFragmentShader.hlsl");
        TransparentMimicShader  = new TransparentMimicShader(DirectXData, MirrorServices, Factory);
        MirrorShader            = new MirrorShader          (DirectXData, MirrorServices, Factory);

        ShadedModelShader       = new Shader(DirectXData, MirrorServices, Factory, "ShadedVertexShader.hlsl",       "ShadedFragmentShader.hlsl",    [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)]);
    }

    public void Dispose()
    {
        AlphaShader?.Dispose();
        ClippedShader?.Dispose();
        InvertAlphaShader?.Dispose();
        TransparentMimicShader?.Dispose();
        MirrorShader?.Dispose();
        ShadedModelShader?.Dispose();
    }
}
