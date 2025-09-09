using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using MirrorsEdge.MirrorsEdge.Hooking;
using MirrorsEdge.MirrorsEdge.Services;
using System.Numerics;

namespace MirrorsEdge.mirrorsedge.Hooking.HookableElements;

internal class ScreenHook : HookableElement
{
    public ScreenHook(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
    }

    public override void Init()
    {
        DalamudServices.Framework.Update += OnScreenHookUpdate;
    }

    private void OnScreenHookUpdate(IFramework framework)
    {
        Vector2 size = ImGuiHelpers.MainViewport.Size;

        MirrorServices.MirrorLog.Log(size);
    }

    public override void OnDispose()
    {
        DalamudServices.Framework.Update -= OnScreenHookUpdate;
    }
}
