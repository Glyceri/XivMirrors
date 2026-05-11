using System;
using Dalamud.Interface.Windowing;
using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;
using TheMirrorsEdge.Windowing.Windows;

namespace TheMirrorsEdge.Windowing;

public class WindowHandler : IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    private readonly WindowSystem    WindowSystem;
    private readonly HookHandler     HookHandler;
    
    private readonly DebugWindow     DebugWindow;
    
    public WindowHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, HookHandler hookHandler, ShaderHandler shaderHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        HookHandler     = hookHandler;
        WindowSystem    = new WindowSystem("TheMirrorsEdge");
        
        DalamudServices.DalamudPlugin.UiBuilder.Draw += Draw;
        
        WindowSystem.AddWindow(DebugWindow = new DebugWindow(DalamudServices, MirrorServices, HookHandler, shaderHandler));
    }
    
    private void Draw()
    {
        HookHandler.ScreenHook.OnImGuiDraw();
        //MirrorServices.MirrorLog.LogVerbose("POST IMGUI DRAW");
        WindowSystem.Draw();
    }
    
    public void Dispose()
    {
        DebugWindow.Dispose();
        
        WindowSystem.RemoveAllWindows();
        
        DalamudServices.DalamudPlugin.UiBuilder.Draw -= Draw;
    }
}