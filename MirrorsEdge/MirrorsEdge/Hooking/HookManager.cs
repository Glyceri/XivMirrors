using MirrorsEdge.mirrorsedge.Hooking.HookableElements;
using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Hooking.Interfaces;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Shaders;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Hooking;

internal class HookManager : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly DirectXData        DirectXData;
    private readonly ShaderHandler      ShaderHandler;

    private readonly List<IHookableElement> _hookableElements = new List<IHookableElement>();

    public readonly CameraHooks         CameraHooks;
    public readonly RendererHook        RendererHook;
    public readonly ScreenHook          ScreenHook;
    public readonly BackBufferHook      BackBufferHook;

    public HookManager(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, ShaderHandler shaderHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        DirectXData     = directXData;
        ShaderHandler   = shaderHandler;

        Register(CameraHooks    = new CameraHooks(DalamudServices, MirrorServices));
        Register(RendererHook   = new RendererHook(DalamudServices, MirrorServices));
        Register(ScreenHook     = new ScreenHook(DalamudServices, MirrorServices));
        Register(BackBufferHook = new BackBufferHook(DalamudServices, MirrorServices, DirectXData, RendererHook, ScreenHook, ShaderHandler));
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
