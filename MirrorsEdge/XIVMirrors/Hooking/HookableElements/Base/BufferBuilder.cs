using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.shaders.ShaderTypes;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements.Base;

internal unsafe abstract class BufferBuilder : HookableElement
{
    protected readonly DirectXData    DirectXData;
    protected readonly RendererHook   RendererHook;
    protected readonly ScreenHook     ScreenHook;
    protected readonly ShaderHandler  ShaderHandler;

    protected uint CurrentScreenWidth  { get; private set; }
    protected uint CurrentScreenHeight { get; private set; }

    protected BufferBuilder(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        RendererHook.RegisterRenderPassListener(OnRenderPass);
        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);
    }

    public sealed override void Init()
    {


        OnInit();
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Pre)
        {
            LocalPreRenderPass();

            OnPreRenderPass();
        }
        else if (renderPass == RenderPass.Post)
        {
            OnPostRenderPass();

            LocalPostRenderPass();
        }
    }

    private void LocalPreRenderPass()
    {
        try
        {
            if (!SetupBuffers())
            {
                return;
            }
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        try
        {
            HandleShaders();
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    private void LocalPostRenderPass()
    {
        // Irrelevant
    }

    private void OnScreenSizeChanged(uint newWidth, uint newHeight)
    {
        CurrentScreenWidth  = newWidth;
        CurrentScreenHeight = newHeight;

        DisposeOldBuffers();
    }

    protected bool OverrideMappedTexture(ref MappedTexture? mappedTexture, ref RenderTarget? renderTarget, Texture* texture)
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

    protected bool CreateMappedTexture(Texture* texture, [NotNullWhen(true)] out MappedTexture? mappedTexture)
    {
        mappedTexture = null;

        if (texture == null)
        {
            return false;
        }

        mappedTexture = new MappedTexture(DirectXData, texture);

        return true;
    }

    protected void RunPastImageMappedShader(ImageMappedShader shader, ref MappedTexture? mappedTexture, RenderTarget? renderTarget)
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

    protected virtual void OnInit()             { }
    protected virtual void OnPreRenderPass()    { }
    protected virtual void OnPostRenderPass()   { }
    protected virtual void HandleShaders()      { }

    protected abstract bool SetupBuffers();
    protected abstract void DisposeOldBuffers();
    protected abstract void DisposeFinal();

    protected sealed override void OnDispose()
    {
        DisposeOldBuffers();

        RendererHook.DeregisterRenderPassListener(OnRenderPass);
        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);

        DisposeFinal();
    }
}
