using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace MirrorsEdge.XIVMirrors.shaders.ShaderTypes;

public class ImageMappedShader : Shader
{
    private readonly ScaledResolutionBuffer scaledResolutionBuffer;

    public ImageMappedShader(MirrorServices mirrorServices, ShaderFactory factory, string fragmentFile) 
        : base(mirrorServices, factory, "ImageMapperVertexShader.hlsl", fragmentFile, [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)])
    {
        scaledResolutionBuffer = new ScaledResolutionBuffer(mirrorServices.DirectXData);
    }

    public void Bind(MappedTexture mappedTexture, RenderTexture? renderTarget = null)
    {
        Bind(0);

        Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width, (int)mappedTexture.Height);

        MirrorServices.DirectXData.Context.Rasterizer.SetViewport(viewport);

        scaledResolutionBuffer.UpdateBuffer(mappedTexture.ScaledResolution);

        scaledResolutionBuffer.BindToVertexShader(0);

        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(0, mappedTexture.ShaderResourceView);

        if (renderTarget != null)
        {
            MirrorServices.DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

            MirrorServices.DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(1, 0, 1, 1));
        }

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
