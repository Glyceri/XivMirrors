using System;
using System.Collections.Generic;
using TheMirrorsEdge.Hooking.Elements;
using TheMirrorsEdge.Hooking.Interfaces;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Hooking;

public class HookHandler : IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    
    private readonly List<IHookableElement> HookableElements = [];
    
    public IScreenHook ScreenHook 
        { get; private set; } = null!;
    
    public CameraHook CameraHook
        { get; private set; } = null!;
    
    public HookHandler(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        
        _Register();
        _Init();
    }
    
    private void _Register()
    {
        Register(CameraHook = new CameraHook(DalamudServices, MirrorServices));
        Register(new CharacterManagerHook(DalamudServices, MirrorServices));
        Register(ScreenHook = new ScreenHook(DalamudServices, MirrorServices));
        Register(new RenderHook(DalamudServices, MirrorServices));
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