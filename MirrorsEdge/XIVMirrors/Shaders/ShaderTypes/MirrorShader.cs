using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace MirrorsEdge.XIVMirrors.shaders.ShaderTypes;

internal class MirrorShader : Shader
{
    public MirrorShader(DirectXData data, MirrorServices mirrorServices, ShaderFactory factory) : base(data, mirrorServices, factory, "ImageMapperVertexShader.hlsl", "MirrorFragmentShader.hlsl", [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)])
    {

    }

    public void Bind
        (
            MappedTexture depthTextureNoTransparency, 
            MappedTexture depthTextureWithTransparency,
            MappedTexture backBuffer,
            MappedTexture backBufferNoUI,
            MappedTexture modelMap,
            MappedTexture modelDepthMap,
            RenderTarget renderTarget
        )
    {
        Bind(0);

        Viewport viewport = new Viewport(0, 0, (int)renderTarget.Width, (int)renderTarget.Height);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        depthTextureNoTransparency.UpdateConstantBuffer(DirectXData);

        DirectXData.Context.VertexShader.SetConstantBuffer(0, depthTextureNoTransparency.ConstantBuffer);

        DirectXData.Context.PixelShader.SetShaderResource(0, depthTextureNoTransparency.ShaderResourceView);
        DirectXData.Context.PixelShader.SetShaderResource(1, depthTextureWithTransparency.ShaderResourceView);
        DirectXData.Context.PixelShader.SetShaderResource(2, backBuffer.ShaderResourceView);
        DirectXData.Context.PixelShader.SetShaderResource(3, backBufferNoUI.ShaderResourceView);
        DirectXData.Context.PixelShader.SetShaderResource(4, modelMap.ShaderResourceView);
        DirectXData.Context.PixelShader.SetShaderResource(5, modelDepthMap.ShaderResourceView);

        DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

        DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(0, 0, 0, 0));

        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }

    public void Draw()
    {
        DirectXData.Context.Draw(6, 0);
    }

    public void UnbindTexture()
    {
        DirectXData.Context.PixelShader.SetShaderResource(lastBoundSlot, null);

        DirectXData.Context.OutputMerger.ResetTargets();
    }
}
