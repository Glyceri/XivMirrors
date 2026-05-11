using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using TheMirrorsEdge.Camera;
using TheMirrorsEdge.Camera.CameraTypes;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;
using TheMirrorsEdge.Services.Screenshotting.ScreenshotTypes;

namespace TheMirrorsEdge.Windowing.Windows;

public class DebugWindow : MirrorWindow
{
    protected override Vector2 MinSize     => new Vector2(350, 136);
    protected override Vector2 MaxSize     => new Vector2(3000, 3000);
    protected override Vector2 DefaultSize => new Vector2(800, 400);
    
    private ulong _drawCount = 0;
    
    private readonly CameraHandler CameraHandler;
    
    private List<IScreenshot> Screenshots = [];
    
    private IScreenshot? ActiveScreenshot = null;
    
    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler) 
        : base(windowHandler, dalamudServices, mirrorServices, "DebugWindow") 
    {
        CameraHandler = cameraHandler;
        
        MirrorServices.RenderService.RegisterRenderListener(OnRender);
        
        Open();
    }
    
    protected override void OnDispose()
    {
        foreach (IScreenshot screenshot in Screenshots)
        {
            screenshot.Dispose();
        }
        
        Screenshots.Clear();
        
        MirrorServices.RenderService.DeregisterRenderListener(OnRender);
    }
    
    protected override void OnDraw()
    { 
        ImGui.Text(_drawCount.ToString());
        
        ImGui.Separator();
        
        if (ImGui.Button("Take Screenshot"))
        {
            Screenshots.Add(new BackBufferScreenshot(DalamudServices, MirrorServices));
        }
        
        int sIndex = 0;
        
        for (int i = Screenshots.Count - 1; i >= 0; i--)
        {
            IScreenshot screenshot = Screenshots[i];
            
            if (!screenshot.ScreenshotReady)
            {
                continue;
            }
            
            sIndex++;
            
            if (ImGui.Button($"Set Active [{screenshot.GUID}]###Screenshot{sIndex}"))
            {
                ActiveScreenshot = screenshot;
            }
            
            ImGui.SameLine();
            
            if (ImGui.Button($"X###ScreenshotX{sIndex}"))
            {
                if (ActiveScreenshot == screenshot)
                {
                    ActiveScreenshot = null;
                }
                
                Screenshots.RemoveAt(i);
            }
        }
        
        if (ActiveScreenshot != null)
        {
            foreach (IScreenshotFile screenshotFile in ActiveScreenshot.ScreenshotFiles)
            {
                if (screenshotFile.ScreenshotTexture == null)
                {
                    return;
                }
                
                ImGui.ImageButton(screenshotFile.ScreenshotTexture.Handle, new Vector2(320, 256));
            }
        }
        
        ImGui.Separator();
        
        if (ImGui.Button("Create Camera"))
        {
            CameraHandler.CreateCamera();
        }
        
        BaseCamera? activeCamera = CameraHandler.ActiveCamera;
        
        int index = 0;
        
        for (int i = CameraHandler.Cameras.Length - 1; i >= 0; i--)
        {
            BaseCamera camera = CameraHandler.Cameras[i];
            
            index++;
            ImGui.BeginDisabled(camera == activeCamera);
            
            if (ImGui.Button($"[{camera.GetType().Name}] Set Active###CAMERA{index}"))
            {
                CameraHandler.SetActiveCamera(camera);
            }
            
            ImGui.SameLine();
            
            ImGui.EndDisabled();
            
            ImGui.BeginDisabled(camera is NativeCamera);
            
            if (ImGui.Button($"X##XBUTTONCAMERA{index}"))
            {
                CameraHandler.DestroyCamera(camera);
            }
            
            ImGui.EndDisabled();
        }
    }
    
    private void OnRender()
        => _drawCount++;
}