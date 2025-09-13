using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    public  delegate void RenderPassDelegate(RenderPass pass);
    private delegate void DXGIPresentDelegate(IntPtr ptr);

    private delegate void RenderThreadSetRenderTargetDelegate(Device* deviceInstance, SetRenderTargetCommand* command);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 5F 18", DetourName = nameof(RenderThreadSetRenderTargetDetour))]
    private Hook<RenderThreadSetRenderTargetDelegate>? RenderThreadSetRenderTargetHook = null;

    [Signature("E8 ?? ?? ?? ?? C6 46 79 00 48 8B 8E 88 0A 0E 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;

    private readonly List<RenderPassDelegate> _renderPasses = new List<RenderPassDelegate>();

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
        
    }

    public override void Init()
    {
        DXGIPresentHook?.Enable();
        RenderThreadSetRenderTargetHook?.Enable();
    }

    private void DXGIPresentDetour(IntPtr ptr)
    {
        MirrorServices.MirrorLog.LogVerbose($"Draw: {UniqueDepthBuffers.Count}");

        //UniqueDepthBuffers.Clear();

        try
        {
            foreach (RenderPassDelegate renderPass in _renderPasses)
            {
                renderPass?.Invoke(RenderPass.Pre);
            }

            DXGIPresentHook!.Original(ptr);

            foreach (RenderPassDelegate renderPass in _renderPasses)
            {
                renderPass?.Invoke(RenderPass.Post);
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }
    }

    private readonly List<nint> UniqueDepthBuffers = new List<nint>();

    private void RenderThreadSetRenderTargetDetour(Device* deviceInstance, SetRenderTargetCommand* command)
    {
        try
        {
            RenderThreadSetRenderTargetHook!.Original(deviceInstance, command);

            if (command->DepthBuffer != null)
            {
                nint dBuffer = (nint)command->DepthBuffer;


                _ = UniqueDepthBuffers.Remove(dBuffer);
                UniqueDepthBuffers.Add(dBuffer);
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }
    }

    public void RegisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = _renderPasses.Remove(renderDelegate);

        _renderPasses.Add(renderDelegate);
    }

    public void DeregisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = _renderPasses.Remove(renderDelegate);
    }

    public override void OnDispose()
    {
        _renderPasses.Clear();

        RenderThreadSetRenderTargetHook?.Disable();
        RenderThreadSetRenderTargetHook?.Dispose();

        DXGIPresentHook?.Disable();
        DXGIPresentHook?.Dispose();
    }
}
