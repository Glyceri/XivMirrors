using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
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

    private Texture2D?      backer;
    private DepthTexture?   depthTexture;
    private RenderTarget?   renderTarget;

    private readonly BasicModel   CubeModel;
    private readonly MatrixBuffer MatrixBuffer;

    private readonly IDalamudTextureWrap TextureWrap;
    private readonly ShaderResourceView  TextureResourceView;

    private uint currentScreenWidth;
    private uint currentScreenHeight;

    private readonly RasterizerState   RasterizerState;
    private readonly DepthStencilState DepthStencilState;

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

        PrimitiveDeclaration cube = PrimitiveFactory.Cube();

        CubeModel       = new BasicModel(DirectXData, ref cube);
        MatrixBuffer    = new MatrixBuffer(DirectXData);

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
            DepthComparison = Comparison.GreaterEqual
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
        Matrix cameraMatrix = CameraHook.GetMatrix();

        MatrixBuffer.UpdateBuffer(ref cameraMatrix);

        MatrixBuffer.Bind();
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
        DepthStencilState?.Dispose();
        RasterizerState?.Dispose();

        TextureResourceView?.Dispose();
        TextureWrap?.Dispose();

        backer?.Dispose();
        renderTarget?.Dispose();
        CubeModel?.Dispose();
        MatrixBuffer?.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
