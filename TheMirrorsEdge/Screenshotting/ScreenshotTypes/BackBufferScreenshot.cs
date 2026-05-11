using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using TheMirrorsEdge.CSClone;
using TheMirrorsEdge.Screenshotting.ScreenshotFiles;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.Screenshotting;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Screenshotting.ScreenshotTypes;

public unsafe class BackBufferScreenshot : Screenshot
{
    private static MyRenderTargetManager* MirrorsRenderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();
    
    public BackBufferScreenshot(DalamudServices dalamudServices, MirrorServices mirrorServices, ShaderHandler shaderHandler) 
        : base(GetFromBackBuffer(dalamudServices, mirrorServices, shaderHandler)) { }
    
    private static IScreenshotFile[] GetFromBackBuffer(DalamudServices dalamudServices, MirrorServices mirrorServices, ShaderHandler shaderHandler)
    {
        IScreenshotFile deviceBackBuffer  = new ScreenshotFile(mirrorServices, shaderHandler, MirrorsRenderTargetManager->DeviceBackBuffer);
        IScreenshotFile backBufferNoUI    = new ScreenshotFile(mirrorServices, shaderHandler, MirrorsRenderTargetManager->BackBufferNoUI);
        IScreenshotFile dalamudBackBuffer = new ViewportScreenshotFile(mirrorServices, shaderHandler, dalamudServices);
        IScreenshotFile rawBackBuffer     = new BackBufferScreenshotFile(mirrorServices, shaderHandler);
        IScreenshotFile rawPostBackBuffer = new BackBufferScreenshotFile(mirrorServices, shaderHandler, false);
        
        return [deviceBackBuffer, backBufferNoUI, dalamudBackBuffer, rawBackBuffer, rawPostBackBuffer];
    }
}