using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Services;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    public delegate bool RenderPassDelegate(RenderPass pass);

    private RenderPassDelegate? renderPass = null!;

    private delegate void DXGIPresentDelegate(long a, long b);

    [Signature("E8 ?? ?? ?? ?? C6 43 79 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;

    public bool StopRenderer = false;

    private readonly CameraHooks CameraHooks;

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHooks cameraHooks) : base(dalamudServices, mirrorServices)
    {
        CameraHooks = cameraHooks;
    }

    public override void Init()
    {
        DXGIPresentHook?.Enable();
    }

    private void DXGIPresentDetour(long a, long b)
    {
        if (renderPass?.Invoke(RenderPass.Mirror) ?? false)
        {
            DXGIPresentHook!.Original(a, b);
        }

        CameraHooks.SetOverride(null);

        if (renderPass?.Invoke(RenderPass.Main) ?? true)
        {
            DXGIPresentHook!.Original(a, b);
        }
    }

    public void SetRenderPassListener(RenderPassDelegate? renderDelegate)
    {
        renderPass = renderDelegate;
    }

    public override void OnDispose()
    {
        DXGIPresentHook?.Dispose();
    }
}
