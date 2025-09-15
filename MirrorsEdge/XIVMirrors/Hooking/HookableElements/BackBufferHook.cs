using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Mirrors;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class BackBufferHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;

    private readonly Mirror Mirror;

    private MappedTexture? backBufferWithUI;
    private MappedTexture? backBufferNoUI;
    private MappedTexture? nonTransparentDepthBuffer;
    private MappedTexture? transparentDepthBuffer;

    public MappedTexture? BackBufferWithUI              => backBufferWithUI;
    public MappedTexture? BackBufferNoUI                => backBufferNoUI;
    public MappedTexture? DepthBufferNoTransparency     => nonTransparentDepthBuffer;
    public MappedTexture? DepthBufferWithTransparency   => transparentDepthBuffer;

    public BackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        Mirror = new Mirror(directXData);

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);
    }

    public override void Init()
    {
        DalamudServices.Framework.RunOnFrameworkThread(() => RendererHook.RegisterRenderPassListener(OnRenderPass));
    }

    private void OnScreenSizeChanged(int newWidth, int newHeight)
    {
        DisposeOldBuffers();
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        try
        {
            if (!SetupBuffers())
            {
                return;
            }
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogError(e, "Fat chance your game just crashed.");
        }
    }

    private bool OverrideMappedTexture(ref MappedTexture? mappedTexture, Texture* texture)
    {
        if (mappedTexture != null)
        {
            return true;
        }

        if (CreateMappedTexture(texture, out MappedTexture? newTexture))
        {
            mappedTexture = newTexture;

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

        mappedTexture = new MappedTexture(texture);

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

        failed |= !OverrideMappedTexture(ref backBufferWithUI,          renderTargetManager->BackBuffer);
        failed |= !OverrideMappedTexture(ref backBufferNoUI,            renderTargetManager->BackBufferNoUI);
        failed |= !OverrideMappedTexture(ref nonTransparentDepthBuffer, renderTargetManager->DepthBufferNoTransparency);
        failed |= !OverrideMappedTexture(ref transparentDepthBuffer,    renderTargetManager->DepthBufferTransparency);

        return failed;
    }

    private void DisposeOldBuffers()
    {
        backBufferWithUI?.Dispose();
        backBufferWithUI?.Dispose();
        nonTransparentDepthBuffer?.Dispose();
        transparentDepthBuffer?.Dispose();

        backBufferWithUI = null;
        backBufferNoUI = null;
        nonTransparentDepthBuffer = null;
        transparentDepthBuffer = null;
    }

    public override void OnDispose()
    {
        Mirror.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
