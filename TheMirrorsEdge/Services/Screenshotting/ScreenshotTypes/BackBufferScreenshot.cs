using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using TheMirrorsEdge.CSClone;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;

namespace TheMirrorsEdge.Services.Screenshotting.ScreenshotTypes;

public unsafe class BackBufferScreenshot : Screenshot
{
    private static MyRenderTargetManager* MirrorsRenderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();
    
    public BackBufferScreenshot(DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(GetFromBackBuffer(dalamudServices, mirrorServices)) { }
    
    private static IScreenshotFile[] GetFromBackBuffer(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        IScreenshotFile deviceBackBuffer  = new ScreenshotFile(mirrorServices.DirectXData, MirrorsRenderTargetManager->DeviceBackBuffer);
        IScreenshotFile backBufferNoUI    = new ScreenshotFile(mirrorServices.DirectXData, MirrorsRenderTargetManager->BackBufferNoUI);
        IScreenshotFile dalamudBackBuffer = new ViewportScreenshotFile(dalamudServices, mirrorServices);
        IScreenshotFile rawBackBuffer     = new BackBufferScreenshotFile(dalamudServices, mirrorServices);
        IScreenshotFile rawPostBackBuffer = new BackBufferScreenshotFile(dalamudServices, mirrorServices, false);
        
        return [deviceBackBuffer, backBufferNoUI, dalamudBackBuffer, rawBackBuffer, rawPostBackBuffer];
    }
}