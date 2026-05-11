using SharpDX.Direct3D11;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;

namespace TheMirrorsEdge.Services.Screenshotting;

public class BackBufferScreenshotFile : IScreenshotFile
{
    public MappedTexture? ScreenshotTexture { get; private set; }
    public bool           ScreenshotReady   { get; private set; }
    
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    
    public BackBufferScreenshotFile(DalamudServices dalamudServices, MirrorServices mirrorServices, bool asPrePresent = true)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        
        if (asPrePresent)
        {
            MirrorServices.RenderService.RegisterPrePresentListener(PrePresent);
        }
        else
        {
            MirrorServices.RenderService.RegisterPostPresentListener(PrePresent);
        }
    }
    
    private void PrePresent()
    {
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
        MirrorServices.RenderService.DeregisterPostPresentListener(PrePresent);
        
        Texture2D? backBuffer  = MirrorServices.DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);
        
        if (backBuffer == null)
        {
            return;
        }
        
        Texture2DDescription desc   = backBuffer.Description;

        desc.BindFlags              = BindFlags.ShaderResource | BindFlags.RenderTarget;
        desc.Usage                  = ResourceUsage.Default;
        desc.CpuAccessFlags         = CpuAccessFlags.None;

        Texture2D backBufferCopy    = new Texture2D(MirrorServices.DirectXData.Device, desc);

        MirrorServices.DirectXData.Context.CopyResource(backBuffer, backBufferCopy);
        
        ShaderResourceView srv      = new ShaderResourceView(MirrorServices.DirectXData.Device, backBufferCopy);
        
        ScreenshotTexture           = new MappedTexture(ref backBufferCopy, ref srv);
        ScreenshotReady             = true;
    }
    
    public void Dispose()
    {
        ScreenshotTexture?.Dispose();
        
        MirrorServices.RenderService.DeregisterPostPresentListener(PrePresent);
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
    }
}