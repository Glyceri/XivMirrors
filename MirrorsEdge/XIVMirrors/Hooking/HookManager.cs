using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Hooking.Interfaces;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using System;
using System.Collections.Generic;
using MirrorsEdge.XIVMirrors.ResourceHandling;

namespace MirrorsEdge.XIVMirrors.Hooking;

internal class HookManager : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly DirectXData        DirectXData;
    private readonly ShaderHandler      ShaderHandler;
    private readonly ResourceHandler    ResourceHandler;

    private readonly List<IHookableElement> _hookableElements = new List<IHookableElement>();

    public readonly CameraHooks         CameraHooks;
    public readonly RendererHook        RendererHook;
    public readonly ScreenHook          ScreenHook;
    public readonly BackBufferHook      BackBufferHook;
    public readonly ResourceHooks       ResourceHooks;
    public ThatShitFromKara    ThatShitFromKara;

    public HookManager(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, ShaderHandler shaderHandler, ResourceHandler resourceHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        DirectXData     = directXData;
        ShaderHandler   = shaderHandler;
        ResourceHandler = resourceHandler;

        Register(CameraHooks        = new CameraHooks(DalamudServices, MirrorServices));
        Register(RendererHook       = new RendererHook(DalamudServices, MirrorServices, DirectXData));
        Register(ScreenHook         = new ScreenHook(DalamudServices, MirrorServices, RendererHook));
        Register(BackBufferHook     = new BackBufferHook(DalamudServices, MirrorServices, DirectXData, RendererHook, ScreenHook, ShaderHandler));
        Register(ResourceHooks      = new ResourceHooks(DalamudServices, MirrorServices, ResourceHandler));
        
        //DalamudServices.Framework.RunOnTick(() => Register(ThatShitFromKara   = new ThatShitFromKara(DalamudServices, MirrorServices, directXData)));
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
