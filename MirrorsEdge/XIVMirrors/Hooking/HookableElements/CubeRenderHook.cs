using Dalamud.Interface.Textures.TextureWraps;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Rendering.Structs;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using PrimitiveDeclaration = (MirrorsEdge.XIVMirrors.Rendering.Vertex[] vertices, ushort[] indices);

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal class CubeRenderHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly CameraHooks    CameraHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;
    private readonly BackBufferHook BackBufferHook;

    private Texture2D?      backer;
    private DepthTexture?   depthTexture;
    private RenderTarget?   renderTarget;

    private readonly BasicModel   CubeModel;
    private readonly CameraBuffer CameraBuffer;

    private readonly IDalamudTextureWrap TextureWrap;
    private readonly ShaderResourceView  TextureResourceView;

    private uint currentScreenWidth;
    private uint currentScreenHeight;

    private readonly RasterizerState   RasterizerState;
    private readonly DepthStencilState DepthStencilState;

    private MappedTexture? backBufferWithUICopy;
    private MappedTexture? backBufferNoUICopy;
    private MappedTexture? nonTransparentDepthBufferCopy;
    private MappedTexture? transparentDepthBufferCopy;
    private MappedTexture? cubeTextureCopy;
    private MappedTexture? depthTextureCopy;

    private RenderTarget?  finalRenderTarget;

    public ShaderResourceView? OutputView
        => renderTarget?.ShaderResourceView;

    public ShaderResourceView? DepthView
        => depthTexture?.ShaderResourceView;

    public MappedTexture? BackBufferWithUICopy
        => backBufferWithUICopy;

    public MappedTexture? BackBufferNoUICopy
        => backBufferNoUICopy;

    public MappedTexture? NonTransparentDepthBufferCopy
        => nonTransparentDepthBufferCopy;

    public MappedTexture? TransparentDepthBufferCopy
        => transparentDepthBufferCopy;

    public RenderTarget? FinalRenderTarget
        => finalRenderTarget;

    public CubeRenderHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook renderHook, CameraHooks cameraHook, ScreenHook screenHook, BackBufferHook backBufferHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = renderHook;
        CameraHook      = cameraHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;
        BackBufferHook  = backBufferHook;

        RendererHook.RegisterRenderPassListener(OnRenderPass);
        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);

        PrimitiveDeclaration cube = PrimitiveFactory.Cube();

        CubeModel       = new BasicModel(DirectXData, ref cube);
        CameraBuffer    = new CameraBuffer(DirectXData);

        TextureWrap     = DalamudServices.TextureProvider.GetFromFileAbsolute("C:\\Users\\Amber\\PetRenamer\\MirrorsEdge\\MirrorsEdge\\XIVMirrors\\shaders\\files\\nightsky.png").RentAsync().Result;
        TextureResourceView = new ShaderResourceView((nint)TextureWrap.Handle.Handle);

        RasterizerStateDescription rsDesc = new RasterizerStateDescription
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            IsFrontCounterClockwise = true // flip front face
        };

        RasterizerState = new RasterizerState(DirectXData.Device, rsDesc);

        DepthStencilStateDescription dsDesc = new DepthStencilStateDescription
        {
            IsDepthEnabled  = true,
            DepthWriteMask  = DepthWriteMask.All,
            DepthComparison = Comparison.Greater
        };

        DepthStencilState = new DepthStencilState(DirectXData.Device, dsDesc);
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

        DirectXData.Context.OutputMerger.SetRenderTargets(depthTexture.DepthStencilView, renderTarget.RenderTargetView);

        Viewport viewport = new Viewport(0, 0, (int)currentScreenWidth, (int)currentScreenHeight, 0.0f, 1.0f);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        ShaderHandler.ShadedModelShader.Bind();

        CubeModel.BindBuffer();

        // BIND TEXTURE
        DirectXData.Context.PixelShader.SetShaderResource(0, TextureResourceView);
        // END BIND

        // BIND MATRIX
        CameraBufferLayout cameraMatrix = CameraHook.GetCameraBufferLayout(Matrix.Identity);

        CameraBuffer.UpdateBuffer(ref cameraMatrix);

        CameraBuffer.Bind();
        // END BIND

        DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(0, 0, 0, 0));
        DirectXData.Context.ClearDepthStencilView(depthTexture.DepthStencilView, DepthStencilClearFlags.Depth, 0.0f, 0);

        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        DirectXData.Context.Rasterizer.State = RasterizerState;

        DirectXData.Context.OutputMerger.DepthStencilState = DepthStencilState;

        CubeModel.Draw();

        DirectXData.Context.OutputMerger.ResetTargets();

        ShaderHandler.ShadedModelShader.Release();

        DirectXData.Context.Rasterizer.State = null;
        DirectXData.Context.OutputMerger.DepthStencilState = null;
        DirectXData.Context.OutputMerger.SetBlendState(null);




        backBufferWithUICopy?.Dispose();
        backBufferNoUICopy?.Dispose();
        nonTransparentDepthBufferCopy?.Dispose();
        transparentDepthBufferCopy?.Dispose();
        cubeTextureCopy?.Dispose();
        depthTextureCopy?.Dispose();

        finalRenderTarget?.Dispose();

        backBufferWithUICopy            = BackBufferHook?.BackBufferWithUI?.ToMappedTexture(DirectXData);
        backBufferNoUICopy              = BackBufferHook?.BackBufferNoUI?.ToMappedTexture(DirectXData);
        nonTransparentDepthBufferCopy   = BackBufferHook?.DepthBufferNoTransparency?.ToMappedTexture(DirectXData);
        transparentDepthBufferCopy      = BackBufferHook?.DepthBufferWithTransparency?.ToMappedTexture(DirectXData);
        cubeTextureCopy                 = renderTarget?.ToMappedTexture(DirectXData);
        depthTextureCopy                = depthTexture?.ToMappedTexture(DirectXData);

        finalRenderTarget               = backBufferWithUICopy?.CreateRenderTarget(DirectXData);

        FinalShader();
    }


    private void FinalShader()
    {
        if (backBufferWithUICopy == null)
        {
            return;
        }

        if (backBufferNoUICopy == null)
        {
            return;
        }

        if (nonTransparentDepthBufferCopy == null)
        {
            return;
        }

        if (transparentDepthBufferCopy == null)
        {
            return;
        }

        if (cubeTextureCopy == null)
        {
            return;
        }

        if (depthTextureCopy == null)
        {
            return;
        }

        if (finalRenderTarget == null)
        {
            return;
        }

        ShaderHandler.MirrorShader.Bind(nonTransparentDepthBufferCopy, transparentDepthBufferCopy, backBufferNoUICopy, backBufferWithUICopy, cubeTextureCopy, depthTextureCopy, finalRenderTarget);

        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        ShaderHandler.MirrorShader.Draw();

        ShaderHandler.MirrorShader.UnbindTexture();
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
        backBufferWithUICopy?.Dispose();
        backBufferNoUICopy?.Dispose();
        nonTransparentDepthBufferCopy?.Dispose();
        transparentDepthBufferCopy?.Dispose();
        cubeTextureCopy?.Dispose();
        depthTextureCopy?.Dispose();

        finalRenderTarget?.Dispose();

        DepthStencilState?.Dispose();
        RasterizerState?.Dispose();

        TextureResourceView?.Dispose();
        TextureWrap?.Dispose();

        backer?.Dispose();
        renderTarget?.Dispose();
        CubeModel?.Dispose();
        CameraBuffer?.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
