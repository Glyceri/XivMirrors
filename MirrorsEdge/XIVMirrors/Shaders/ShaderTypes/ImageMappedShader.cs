using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace MirrorsEdge.XIVMirrors.shaders.ShaderTypes;

internal class ImageMappedShader : Shader
{
    public ImageMappedShader(DirectXData data, MirrorServices mirrorServices, ShaderFactory factory, string fragmentFile) : base(data, mirrorServices, factory, "ImageMapperVertexShader.hlsl", fragmentFile, [new("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float, 0, 0), new("TEXCOORD", 0, SharpDX.DXGI.Format.R32G32_Float, InputElement.AppendAligned, 0)])
    {

    }

    public void Bind(MappedTexture mappedTexture, RenderTarget? renderTarget = null)
    {
        Bind(0);

        Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width, (int)mappedTexture.Height);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        mappedTexture.UpdateConstantBuffer(DirectXData);

        DirectXData.Context.VertexShader.SetConstantBuffer(0, mappedTexture.ConstantBuffer);
        DirectXData.Context.PixelShader.SetShaderResource(0, mappedTexture.ShaderResourceView);

        if (renderTarget != null)
        {
            DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

            DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(1, 0, 1, 1));
        }

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
