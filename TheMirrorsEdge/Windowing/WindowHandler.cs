using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using TheMirrorsEdge.Camera;
using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Windowing.Windows;

namespace TheMirrorsEdge.Windowing;

public class WindowHandler : IDisposable
{
    private static int _internalCounter  = 0;
    
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    private readonly WindowSystem    WindowSystem;
    private readonly HookHandler     HookHandler;
    private readonly CameraHandler   CameraHandler;
    
    public WindowHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, HookHandler hookHandler, CameraHandler cameraHandler)
    {
        CameraHandler   = cameraHandler;
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        HookHandler     = hookHandler;
        WindowSystem    = new WindowSystem("TheMirrorsEdge");
        
        DalamudServices.DalamudPlugin.UiBuilder.Draw += Draw;
        
        _Register();
    }
    
    private void _Register()
    {
        RegisterWindow(new DebugWindow(this, DalamudServices, MirrorServices, CameraHandler));     
        RegisterWindow(new ConfigurationWindow(this, DalamudServices, MirrorServices));
    }
    
    private void RegisterWindow(MirrorWindow window)
    {
        WindowSystem.AddWindow(window);
    }
    
    private void Draw()
    {
        _internalCounter = 0;
        
        HookHandler.ScreenHook?.OnImGuiDraw();
        
        MirrorServices.MirrorLog.LogExtremelyVerbose("--- POST IMGUI DRAW ---");
        
        MirrorServices.FileDialogService.Draw();
        
        WindowSystem.Draw();
    }
    
    public void Dispose()
    {
        foreach (IWindow window in WindowSystem.Windows)
        {
            if (window is not IDisposable disposable)
            {
                continue;
            }
            
            disposable.Dispose();
        }

        WindowSystem.RemoveAllWindows();
        
        DalamudServices.DalamudPlugin.UiBuilder.Draw -= Draw;
    }
    
    public static int InternalCounter
        => _internalCounter++;

    // The 16 is because this plugin was made for exlusively dalamud font size 12 (which is font scale 16 in ImGUI).
    // Scaling the whole UI thingy around it seems to work perfectly fine
    public static float FontScale 
        => (ImGui.GetFontSize() / 16.0f);

    public static float GlobalScale
        => ImGuiHelpers.GlobalScale * FontScale;

    public static float BarHeight
        => 30 * GlobalScale;
}