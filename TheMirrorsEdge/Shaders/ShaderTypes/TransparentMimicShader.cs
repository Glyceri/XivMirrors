using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace MirrorsEdge.XIVMirrors.shaders.ShaderTypes;

public class TransparentMimicShader : Shader
{
    private readonly ScaledResolutionBuffer scaledResolutionBuffer;

    public TransparentMimicShader(MirrorServices mirrorServices, ShaderFactory factory) 
        : base(mirrorServices, factory, "ImageMapperVertexShader.hlsl", "TransparencyMimicFragmentShader.hlsl", [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)])
    {
        scaledResolutionBuffer = new ScaledResolutionBuffer(mirrorServices.DirectXData);
    }

    public void Bind
    (
        MappedTexture transparentDiffuseMap,
        MappedTexture transparentStrengthMap,
        MappedTexture transparentDiffuseLightMap,
        MappedTexture transparentRealTimeLightMap,
        MappedTexture transparentSpecularLightMap,
        RenderTexture renderTarget
    )
    {
        Bind(0);

        Viewport viewport = new Viewport(0, 0, (int)renderTarget.Width, (int)renderTarget.Height);

        MirrorServices.DirectXData.Context.Rasterizer.SetViewport(viewport);

        scaledResolutionBuffer.UpdateBuffer(transparentDiffuseMap.ScaledResolution);

        scaledResolutionBuffer.BindToVertexShader(0);

        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(0, transparentStrengthMap.ShaderResourceView);
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(1, transparentDiffuseMap.ShaderResourceView);
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(2, transparentDiffuseLightMap.ShaderResourceView);
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(3, transparentSpecularLightMap.ShaderResourceView);
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(4, transparentRealTimeLightMap.ShaderResourceView);

        MirrorServices.DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

        MirrorServices.DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(0, 0, 0, 0));

        MirrorServices.DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }

    public void Draw()
    {
        MirrorServices.DirectXData.Context.Draw(6, 0);
    }

    public void UnbindTexture()
    {
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(lastBoundSlot, null);

        MirrorServices.DirectXData.Context.OutputMerger.ResetTargets();
    }

    protected override void OnDispose()
    {
        scaledResolutionBuffer?.Dispose();
    }
}
