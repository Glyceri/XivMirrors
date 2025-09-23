using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Memory;
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

    public readonly CameraHooks                 CameraHooks;
    public readonly RendererHook                RendererHook;
    public readonly ScreenHook                  ScreenHook;
    public readonly BackBufferHook              BackBufferHook;
    public readonly CubeRenderHook              CubeRenderHook;
    public readonly ResourceHooks               ResourceHooks;
    public readonly TransparentBackBufferHook   TransparentBackBufferHook;

    public HookManager(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, ShaderHandler shaderHandler, ResourceHandler resourceHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        DirectXData     = directXData;
        ShaderHandler   = shaderHandler;
        ResourceHandler = resourceHandler;

        Register(RendererHook               = new RendererHook(DalamudServices, MirrorServices, DirectXData));
        Register(CameraHooks                = new CameraHooks(DalamudServices, MirrorServices, RendererHook));
        Register(ScreenHook                 = new ScreenHook(DalamudServices, MirrorServices, DirectXData));
        Register(BackBufferHook             = new BackBufferHook(DalamudServices, MirrorServices, DirectXData, RendererHook, ScreenHook, ShaderHandler));
        Register(TransparentBackBufferHook  = new TransparentBackBufferHook(DalamudServices, MirrorServices, DirectXData, RendererHook, ScreenHook, ShaderHandler));
        Register(CubeRenderHook             = new CubeRenderHook(DalamudServices, MirrorServices, DirectXData, RendererHook, CameraHooks, ScreenHook, BackBufferHook, ShaderHandler));
        Register(ResourceHooks              = new ResourceHooks(DalamudServices, MirrorServices, ResourceHandler));

        Initialize();
    }

    private void Register(IHookableElement element)
    {
        _ = _hookableElements.Remove(element);
        _hookableElements.Add(element);
    }

    private void Initialize()
    {
        foreach (IHookableElement hookableElement in _hookableElements)
        {
            hookableElement.Init();
        }
    }

    public void Dispose()
    {
        foreach (IHookableElement hookableElement in _hookableElements)
        {
            hookableElement.Dispose();
        }
    }
}
