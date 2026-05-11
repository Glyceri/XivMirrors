using SharpDX.Direct3D11;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Screenshotting.ScreenshotFiles.Base;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Screenshotting.ScreenshotFiles;

public class BackBufferScreenshotFile : MappedScreenshotFile
{
    private readonly bool AsPrePresent = false;
    
    public BackBufferScreenshotFile(MirrorServices mirrorServices, ShaderHandler shaderHandler, bool asPrePresent = true)
        : base (mirrorServices, shaderHandler)
    {
        AsPrePresent = asPrePresent;
    }

    protected override void OnUIHidden()
    {
        if (AsPrePresent)
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
        
        SetScreenshotFile(new MappedTexture(ref backBufferCopy, ref srv));
    }
    
    protected override void OnDispose()
    {
        MirrorServices.RenderService.DeregisterPostPresentListener(PrePresent);
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
    }
}