using MirrorsEdge.mirrorsedge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Hooking.Interfaces;
using MirrorsEdge.MirrorsEdge.Services;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Hooking;

internal class HookManager : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;

    private readonly List<IHookableElement> _hookableElements = new List<IHookableElement>();

    public readonly CameraHooks         CameraHooks;
    public readonly TextureHooker       TextureHooker;
    public readonly RendererHook        RendererHook;
    public readonly ScreenHook          ScreenHook;

    public HookManager(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;

        Register(CameraHooks    = new CameraHooks(DalamudServices, MirrorServices));
        Register(TextureHooker  = new TextureHooker(DalamudServices, MirrorServices));
        Register(RendererHook   = new RendererHook(DalamudServices, MirrorServices, CameraHooks));
        Register(ScreenHook     = new ScreenHook(DalamudServices, MirrorServices));
    }

    private void Register(IHookableElement element)
    {
        _ = _hookableElements.Remove(element);
        _hookableElements.Add(element);

        element.Init();
    }

    public void Dispose()
    {
        foreach (IHookableElement hookableElement in _hookableElements)
        {
            hookableElement.Dispose();
        }
    }
}
