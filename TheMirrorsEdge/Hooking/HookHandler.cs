using System;
using System.Collections.Generic;
using TheMirrorsEdge.Hooking.Elements;
using TheMirrorsEdge.Hooking.Interfaces;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Hooking;

public class HookHandler : IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    private readonly ShaderHandler   ShaderHandler;
    
    private readonly List<IHookableElement> HookableElements = [];
    
    public IScreenHook ScreenHook 
        { get; private set; }
    
    public RenderHook RenderHook
        { get; private set; }
    
    public CameraHook CameraHook
        { get; private set; }
    
    public HookHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, ShaderHandler shaderHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        ShaderHandler   = shaderHandler;
        
        _Register();
        _Init();
    }
    
    private void _Register()
    {
        Register(CameraHook = new CameraHook(DalamudServices, MirrorServices));
        Register(new CharacterManagerHook(DalamudServices, MirrorServices));
        Register(ScreenHook = new ScreenHook(DalamudServices, MirrorServices));
        Register(new UIRenderHook(DalamudServices, MirrorServices));
        Register(RenderHook = new RenderHook(DalamudServices, MirrorServices, ScreenHook, CameraHook, ShaderHandler));
    }
    
    private void _Init()
    {
        foreach (IHookableElement hookableElement in HookableElements)
        {
            hookableElement.Init();
        }
    }
    
    private void Register(IHookableElement hookableElement)
    {
        HookableElements?.Add(hookableElement);
    }
    
    public void Dispose()
    {
        foreach (IHookableElement hookableElement in HookableElements)
        {
            hookableElement.Dispose();
        }
    }
}