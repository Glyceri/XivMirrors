using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Resources.Interfaces;
using MirrorsEdge.XIVMirrors.Hooking;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using MirrorsEdge.XIVMirrors.Mirrors;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal class BackBufferHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;

    private IRenderTarget? internalRenderTarget;
    public IRenderTarget? BackBuffer { get; private set; }

    private System.Numerics.Vector2 _screenSize = System.Numerics.Vector2.Zero;


    private readonly Mirror Mirror;

    public BackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        Mirror = new Mirror(directXData);

        ScreenHook.SetupSize(ref _screenSize);
        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);
    }

    public override void Init()
    {
        DalamudServices.Framework.RunOnFrameworkThread(() => RendererHook.RegisterRenderPassListener(OnRenderPass));
    }

    private void OnScreenSizeChanged(System.Numerics.Vector2 newScreenSize)
    {
        _screenSize = newScreenSize;
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        try
        {
            GetBackBuffer();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogError(e, "Fat chance your game just crashed.");
        }
    }

    private void InitializeRenderTarget()
    {
        if (BackBuffer != null &&
            BackBuffer.Texture2D != null &&
            BackBuffer.Texture2D.Description.Width == (int)_screenSize.X &&
            BackBuffer.Texture2D.Description.Height == (int)_screenSize.Y)
        {
            return;
        }

        BackBuffer?.Dispose();
        internalRenderTarget?.Dispose();

        Texture2DDescription texture2DDescription = new Texture2DDescription
        {
            Width               = (int)_screenSize.X,
            Height              = (int)_screenSize.Y,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = Format.R8G8B8A8_UNorm,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None,
        };

        BackBuffer              = new RenderTarget(DirectXData, texture2DDescription);
        internalRenderTarget    = new RenderTarget(DirectXData, texture2DDescription);
    }

    private void CopyBackBuffer()
    {
        if (internalRenderTarget == null)
        {
            return;
        }

        if (BackBuffer == null)
        {
            return;
        }

        // ---- back buffer ----
        Texture2D backBuffer = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);

        Texture2DDescription desc = backBuffer.Description;

        desc.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
        desc.Usage = ResourceUsage.Default;
        desc.CpuAccessFlags = CpuAccessFlags.None;

        internalRenderTarget.Texture2D?.Dispose();
        internalRenderTarget.ShaderResourceView?.Dispose();

        internalRenderTarget.Texture2D = new Texture2D(DirectXData.Device, desc);

        DirectXData.Context.CopyResource(backBuffer, internalRenderTarget.Texture2D);

        internalRenderTarget.ShaderResourceView = new ShaderResourceView(DirectXData.Device, internalRenderTarget.Texture2D);
        // ---- end back buffer ----
    }

    private void RemoveAlpha()
    {
        if (internalRenderTarget == null)
        {
            return;
        }

        if (BackBuffer == null)
        {
            return;
        }

        using var backBufferTex = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);
        using var backBufferRTV = new RenderTargetView(DirectXData.Device, backBufferTex);

        DirectXData.Context.OutputMerger.SetRenderTargets(backBufferRTV);

        //Viewport vp = new Viewport(0, 0, internalRenderTarget.Texture2D!.Description.Width, internalRenderTarget.Texture2D!.Description.Height);
        Viewport vp = new Viewport(0, 0, 1280, 720);


        DirectXData.Context.Rasterizer.SetViewport(vp);

        DirectXData.Context.VertexShader.Set(ShaderHandler.MirrorShader.VertexShader);
        DirectXData.Context.PixelShader.Set(ShaderHandler.MirrorShader.FragmentShader);

        DirectXData.Context.PixelShader.SetShaderResource(0, internalRenderTarget.ShaderResourceView!);
        DirectXData.Context.PixelShader.SetSampler(0, ShaderHandler.MirrorShader.SamplerState);

        DirectXData.Context.ClearRenderTargetView(BackBuffer.RenderTargetView, new RawColor4(0, 0, 0, 1.0f));

        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        DirectXData.Context.InputAssembler.InputLayout = null;
        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        DirectXData.Context.Draw(6, 0);
    }

    private void GetBackBuffer()
    {
        InitializeRenderTarget();

        CopyBackBuffer();

        RemoveAlpha();
    }

    public override void OnDispose()
    {
        Mirror.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);

        internalRenderTarget?.Dispose();
        BackBuffer?.Dispose();
    }
}
