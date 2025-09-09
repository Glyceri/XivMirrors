using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Services;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    public  delegate void RenderPassDelegate(RenderPass pass);
    private delegate void DXGIPresentDelegate(IntPtr ptr);

    [Signature("E8 ?? ?? ?? ?? C6 46 79 00 48 8B 8E 88 0A 0E 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;

    private readonly List<RenderPassDelegate> _renderPasses = new List<RenderPassDelegate>();

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
        
    }

    public override void Init()
    {
        DXGIPresentHook?.Enable();
    }

    private void DXGIPresentDetour(IntPtr ptr)
    {
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

        DXGIPresentHook?.Disable();
        DXGIPresentHook?.Dispose();
    }
}
