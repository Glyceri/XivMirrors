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
    public ImageMappedShader(DirectXData data, MirrorServices mirrorServices, ShaderFactory factory, string vertexFile, string fragmentFile, InputElement[] inputElements) : base(data, mirrorServices, factory, vertexFile, fragmentFile, inputElements)
    {

    }

    public void Bind(MappedTexture mappedTexture, RenderTarget renderTarget, int slot = 0, bool clearTarget = true)
    {
        Bind(slot);

        Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width, (int)mappedTexture.Height);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        mappedTexture.UpdateConstantBuffer(DirectXData);

        DirectXData.Context.VertexShader.SetConstantBuffer(slot, mappedTexture.ConstantBuffer);
        DirectXData.Context.PixelShader.SetShaderResource(slot, mappedTexture.ShaderResourceView);

        DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

        if (clearTarget)
        {
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
