using System;
using MirrorsEdge.XIVMirrors.shaders.ShaderTypes;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.ChildServices;

namespace TheMirrorsEdge.Shaders;

public class ShaderHandler : IDisposable
{
    private readonly MirrorServices         MirrorServices;

    public readonly  ImageMappedShader      AlphaShader;
    public readonly  ImageMappedShader      ClippedShader;
    public readonly  ImageMappedShader      InvertAlphaShader;
    public readonly  ChannelMappedShader    ChannelMappedShader;
    public readonly  TransparentMimicShader TransparentMimicShader;
    public readonly  MirrorShader           MirrorShader;
    public readonly  Shader                 ShadedModelShader;

    public readonly ShaderFactory Factory;

    public ShaderHandler(MirrorServices mirrorServices)
    {
        MirrorServices          = mirrorServices;

        Factory                 = new ShaderFactory(MirrorServices);

        AlphaShader             = new ImageMappedShader     (MirrorServices, Factory, "AlphaFragmentShader.hlsl");
        ClippedShader           = new ImageMappedShader     (MirrorServices, Factory, "ClippedFragmentShader.hlsl");
        InvertAlphaShader       = new ImageMappedShader     (MirrorServices, Factory, "InvertAlphaFragmentShader.hlsl");
        ChannelMappedShader     = new ChannelMappedShader   (MirrorServices, Factory);
        TransparentMimicShader  = new TransparentMimicShader(MirrorServices, Factory);
        MirrorShader            = new MirrorShader          (MirrorServices, Factory);

        ShadedModelShader       = new Shader(MirrorServices, Factory, "ShadedVertexShader.hlsl", "ShadedFragmentShader.hlsl", [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)]);
    }

    public void Dispose()
    {
        AlphaShader?.Dispose();
        ClippedShader?.Dispose();
        InvertAlphaShader?.Dispose();
        ChannelMappedShader?.Dispose();
        TransparentMimicShader?.Dispose();
        MirrorShader?.Dispose();
        ShadedModelShader?.Dispose();
    }
}
