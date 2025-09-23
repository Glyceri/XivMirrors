using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements.Base;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.shaders.ShaderTypes;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX.Direct3D11;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class BackBufferHook : BufferBuilder
{
    private Task<IDalamudTextureWrap>? textureWrapTask;

    private IDalamudTextureWrap?    dalamudBackBuffer;
    
    private MappedTexture?          backBufferWithUI;
    private MappedTexture?          backBufferNoUI;
    private MappedTexture?          nonTransparentDepthBuffer;
    private MappedTexture?          transparentDepthBuffer;
    private MappedTexture?          secondDalamudBackBuffer;

    private RenderTarget?           rtBackBufferWithUI;
    private RenderTarget?           rtBackBufferNoUI;
    private RenderTarget?           rtNonTransparentDepthBuffer;
    private RenderTarget?           rtTransparentDepthBuffer;
    private RenderTarget?           rtSecondDalamudBackBuffer;

    private readonly CancellationTokenSource cancellationTokenSource;

    public BackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices, directXData, rendererHook, screenHook, shaderHandler)
    {
        cancellationTokenSource = new CancellationTokenSource();

        DalamudServices.Framework.Update += OnUpdate;
    }

    public MappedTexture? BackBufferWithUIBase
        => backBufferWithUI;

    public MappedTexture? BackBufferNoUIBase
        => backBufferNoUI;

    public MappedTexture? NonTransparentDepthBufferBase
        => nonTransparentDepthBuffer;

    public MappedTexture? TransparentDepthBufferBase
        => transparentDepthBuffer;

    public MappedTexture? SecondDalamudBackBufferBase
        => secondDalamudBackBuffer;



    public RenderTarget? BackBufferWithUI              
        => rtBackBufferWithUI;

    public RenderTarget? BackBufferNoUI                
        => rtBackBufferNoUI;

    public RenderTarget? DepthBufferNoTransparency     
        => rtNonTransparentDepthBuffer;

    public RenderTarget? DepthBufferWithTransparency   
        => rtTransparentDepthBuffer;

    public RenderTarget? SecondDalamudBackBuffer
        => rtSecondDalamudBackBuffer;



    public IDalamudTextureWrap? DalamudBackBuffer             
        => dalamudBackBuffer;

    private void OnUpdate(IFramework framework)
    {
        if (dalamudBackBuffer != null)
        {
            return;
        }

        if (textureWrapTask != null)
        {
            if (!textureWrapTask.IsCompleted)
            {
                return;
            }

            dalamudBackBuffer = textureWrapTask.Result;

            textureWrapTask?.Dispose();
            textureWrapTask = null;

            return;
        }

        textureWrapTask = CreateTextureWrap();
    }

    private Task<IDalamudTextureWrap> CreateTextureWrap()
    {
        MirrorServices.MirrorLog.Log("Created dalamud viewport texture wrap.");

        ImGuiViewportTextureArgs textureArguments = new ImGuiViewportTextureArgs()
        {
            AutoUpdate              = true,
            KeepTransparency        = false,
            TakeBeforeImGuiRender   = true,
            ViewportId              = ImGui.GetMainViewport().ID,
        };

        return DalamudServices.TextureProvider.CreateFromImGuiViewportAsync(textureArguments, cancellationToken: cancellationTokenSource.Token);
    }

    protected override void OnPostRenderPass()
    {
        try
        {
            Texture2D backBuffer  = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);

            Texture2DDescription desc   = backBuffer.Description;

            desc.BindFlags              = BindFlags.ShaderResource | BindFlags.RenderTarget;
            desc.Usage                  = ResourceUsage.Default;
            desc.CpuAccessFlags         = CpuAccessFlags.None;

            Texture2D backBufferCopy    = new Texture2D(DirectXData.Device, desc);

            DirectXData.Context.CopyResource(backBuffer, backBufferCopy);

            ShaderResourceView srv      = new ShaderResourceView(DirectXData.Device, backBufferCopy);

            secondDalamudBackBuffer?.Dispose();

            secondDalamudBackBuffer     = new MappedTexture(DirectXData, ref backBufferCopy, ref srv);

            rtSecondDalamudBackBuffer?.Dispose();

            rtSecondDalamudBackBuffer   = new RenderTarget(DirectXData, backBufferCopy);

            RunPastImageMappedShader(ShaderHandler.AlphaShader, ref secondDalamudBackBuffer, rtSecondDalamudBackBuffer);
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    protected override void HandleShaders()
    {
        RunPastImageMappedShader(ShaderHandler.ClippedShader,  ref backBufferWithUI,           rtBackBufferWithUI);
        RunPastImageMappedShader(ShaderHandler.ClippedShader,  ref backBufferNoUI,             rtBackBufferNoUI);

        RunPastImageMappedShader(ShaderHandler.ClippedShader,  ref nonTransparentDepthBuffer,  rtNonTransparentDepthBuffer);
        RunPastImageMappedShader(ShaderHandler.ClippedShader,  ref transparentDepthBuffer,     rtTransparentDepthBuffer);
    }

    protected override bool SetupBuffers()
    {
        MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();

        if (renderTargetManager == null)
        {
            return false;
        }

        bool failed = false;

        failed |= !OverrideMappedTexture(ref backBufferWithUI,          ref rtBackBufferWithUI,             renderTargetManager->DeviceBackBuffer);
        failed |= !OverrideMappedTexture(ref backBufferNoUI,            ref rtBackBufferNoUI,               renderTargetManager->BackBufferNoUICopy);
        failed |= !OverrideMappedTexture(ref nonTransparentDepthBuffer, ref rtNonTransparentDepthBuffer,    renderTargetManager->DepthBufferNoTransparency);
        failed |= !OverrideMappedTexture(ref transparentDepthBuffer,    ref rtTransparentDepthBuffer,       renderTargetManager->DepthBufferTransparency);

        return !failed;
    }

    protected override void DisposeOldBuffers()
    {
        MirrorServices.MirrorLog.LogVerbose("Disposed buffers");

        dalamudBackBuffer?.Dispose();

        dalamudBackBuffer = null;

        backBufferWithUI?.Dispose();
        backBufferNoUI?.Dispose();
        nonTransparentDepthBuffer?.Dispose();
        transparentDepthBuffer?.Dispose();
        secondDalamudBackBuffer?.Dispose();

        rtBackBufferWithUI?.Dispose();
        rtBackBufferWithUI?.Dispose();
        rtNonTransparentDepthBuffer?.Dispose();
        rtTransparentDepthBuffer?.Dispose();
        rtSecondDalamudBackBuffer?.Dispose();

        backBufferWithUI                = null;
        backBufferNoUI                  = null;
        nonTransparentDepthBuffer       = null;
        transparentDepthBuffer          = null;
        secondDalamudBackBuffer         = null;

        rtBackBufferWithUI              = null;
        rtBackBufferWithUI              = null;
        rtNonTransparentDepthBuffer     = null;
        rtTransparentDepthBuffer        = null;
        rtSecondDalamudBackBuffer       = null;
    }

    protected override void DisposeFinal()
    {
        DalamudServices.Framework.Update -= OnUpdate;

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
    }
}
