using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Resources.Interfaces;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.shaders.ShaderTypes;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class BackBufferHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;

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

    public BackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        cancellationTokenSource = new CancellationTokenSource();

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);

        DalamudServices.Framework.Update += OnUpdate;
    }

    public override void Init()
    {
        _ = DalamudServices.Framework.RunOnFrameworkThread(() => RendererHook.RegisterRenderPassListener(OnRenderPass));
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

    public IDalamudTextureWrap? DalamudBackBuffer             
        => dalamudBackBuffer;

    public RenderTarget? SecondDalamudBackBuffer      
        => rtSecondDalamudBackBuffer;

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

    private void OnScreenSizeChanged(uint newWidth, uint newHeight)
    {
        DisposeOldBuffers();
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Pre)
        {
            OnPreRenderPass();
        }
        else if (renderPass == RenderPass.Post)
        {
            OnPostRenderPass();
        }
    }

    private void OnPostRenderPass()
    {
        try
        {
            using Texture2D backBuffer  = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);

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

            RunPastShader(ShaderHandler.AlphaShader, ref secondDalamudBackBuffer, rtSecondDalamudBackBuffer);
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    private void OnPreRenderPass()
    {
        try
        {
            if (!SetupBuffers())
            {
                return;
            }

            RunPastShader(ShaderHandler.ClippedShader,  ref backBufferWithUI,           rtBackBufferWithUI);
            RunPastShader(ShaderHandler.ClippedShader,  ref backBufferNoUI,             rtBackBufferNoUI);

            RunPastShader(ShaderHandler.ClippedShader,  ref nonTransparentDepthBuffer,  rtNonTransparentDepthBuffer);
            RunPastShader(ShaderHandler.ClippedShader,  ref transparentDepthBuffer,     rtTransparentDepthBuffer);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogError(e, "Fat chance your game just crashed.");
        }
    }

    private void RunPastShader(ImageMappedShader shader, ref MappedTexture? mappedTexture, RenderTarget? renderTarget)
    {
        if (mappedTexture == null)
        {
            return;
        }

        if (!mappedTexture.IsValid)
        {
            mappedTexture?.Dispose();

            mappedTexture = null;

            return;
        }

        if (renderTarget == null)
        {
            return;
        }

        shader.Bind(mappedTexture, renderTarget);

        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        shader.Draw();

        shader.UnbindTexture();
    }

    private bool OverrideMappedTexture(ref MappedTexture? mappedTexture, ref RenderTarget? renderTarget, Texture* texture)
    {
        if (mappedTexture != null && renderTarget != null)
        {
            return true;
        }

        if (CreateMappedTexture(texture, out MappedTexture? newTexture))
        {
            MirrorServices.MirrorLog.LogVerbose("Created buffers");

            mappedTexture?.Dispose();

            mappedTexture = newTexture;

            renderTarget?.Dispose();

            renderTarget = new RenderTarget(DirectXData, mappedTexture.Texture);

            return true;
        }

        return false;
    }

    private bool CreateMappedTexture(Texture* texture, [NotNullWhen(true)] out MappedTexture? mappedTexture)
    {
        mappedTexture = null;

        if (texture == null)
        {
            return false;
        }

        mappedTexture = new MappedTexture(DirectXData, texture);

        return true;
    }

    private bool SetupBuffers()
    {
        MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();

        if (renderTargetManager == null)
        {
            return false;
        }

        bool failed = false;

        failed |= !OverrideMappedTexture(ref backBufferWithUI,          ref rtBackBufferWithUI,             renderTargetManager->BackBuffer);
        failed |= !OverrideMappedTexture(ref backBufferNoUI,            ref rtBackBufferNoUI,               renderTargetManager->BackBufferNoUICopy);
        failed |= !OverrideMappedTexture(ref nonTransparentDepthBuffer, ref rtNonTransparentDepthBuffer,    renderTargetManager->DepthBufferNoTransparency);
        failed |= !OverrideMappedTexture(ref transparentDepthBuffer,    ref rtTransparentDepthBuffer,       renderTargetManager->DepthBufferTransparency);

        return !failed;
    }

    private void DisposeOldBuffers()
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

    public override void OnDispose()
    {
        DalamudServices.Framework.Update -= OnUpdate;

        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();

        DisposeOldBuffers();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
