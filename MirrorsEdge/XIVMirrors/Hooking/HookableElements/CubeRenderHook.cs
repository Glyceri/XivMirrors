using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Resources.Interfaces;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal class CubeRenderHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly CameraHooks    CameraHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;

    private Texture2D?      backer;
    private DepthTexture?   depthTexture;
    private RenderTarget?   renderTarget;

    private uint currentScreenWidth;
    private uint currentScreenHeight;

    public ShaderResourceView? OutputView
        => renderTarget?.ShaderResourceView;

    public ShaderResourceView? DepthView
        => depthTexture?.ShaderResourceView;

    public CubeRenderHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook renderHook, CameraHooks cameraHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = renderHook;
        CameraHook      = cameraHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        RendererHook.RegisterRenderPassListener(OnRenderPass);
        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);
    }

    public override void Init()
    {

    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        if (currentScreenWidth == 0 || currentScreenHeight == 0)
        {
            return;
        }

        if (backer == null || renderTarget == null || depthTexture == null)
        {
            return;
        }

        // https://learn.microsoft.com/en-us/windows/uwp/gaming/create-depth-buffer-resource--view--and-sampler-state

        DirectXData.Context.OutputMerger.SetRenderTargets(depthTexture.DepthStencilView, renderTarget.RenderTargetView);

        Viewport viewport = new Viewport(0, 0, (int)currentScreenWidth, (int)currentScreenHeight, 0.0f, 1.0f);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        // TODO: Create a shader that can render a mother flipflopping cube. Realistically this could render ANY model using the games matrixes :eagersit:
        // Then use that depth buffer and the games depth buffers to do some epic blendy stuff...
        // I hope I get a quadratic depth buffer like the game too :sweat:

        //DirectXData.Context.VertexShader.Set(ShaderHandler.AlphaShader.VertexShader);
        //DirectXData.Context.PixelShader.Set(ShaderHandler.AlphaShader.FragmentShader);

        DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(1, 0, 1, 1f));

        //BlendStateDescription blendDesc = new BlendStateDescription();

        //blendDesc.RenderTarget[0].IsBlendEnabled = false;
        //blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        //DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        //DirectXData.Context.InputAssembler.InputLayout = null;
        //DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        //DirectXData.Context.Draw(3, 0);

        //DirectXData.Context.PixelShader.SetShaderResource(0, null);

        //DirectXData.Context.OutputMerger.ResetTargets();
    }

    private void OnScreenSizeChanged(uint newWidth, uint newHeight)
    {
        currentScreenWidth  = newWidth;
        currentScreenHeight = newHeight;

        backer?.Dispose();
        renderTarget?.Dispose();
        depthTexture?.Dispose();

        Texture2DDescription description = new Texture2DDescription()
        {
            Width               = (int)newWidth,
            Height              = (int)newHeight,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = Format.R32G32B32A32_UInt,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None
        };

        backer          = new Texture2D(DirectXData.Device, description);
        depthTexture    = new DepthTexture(DirectXData, currentScreenWidth, currentScreenHeight);
        renderTarget    = new RenderTarget(DirectXData, backer);

        MirrorServices.MirrorLog.LogInfo("Created render target");
    }

    public override void OnDispose()
    {
        backer?.Dispose();
        renderTarget?.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
