using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.ChildServices;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Screenshotting.ScreenshotFiles.Base;

public abstract class MappedScreenshotFile : IScreenshotFile
{
    private MappedTexture? _temporaryTexture       = null;
    private RenderTexture? _temporaryRenderTexture = null;
 
    protected readonly MirrorServices MirrorServices;
    protected readonly ShaderHandler  ShaderHandler;
    
    private readonly IUIHideHandle IUIHideHandle;
    
    protected MappedScreenshotFile(MirrorServices mirrorServices, ShaderHandler shaderHandler)
    {
        MirrorServices = mirrorServices;
        ShaderHandler  = shaderHandler;
        
        IUIHideHandle = MirrorServices.UIHidingService.HideUI();
        
        MirrorServices.RenderService.RegisterPrePresentListener(PostPresent);
    }
    
    public void Dispose()
    {
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
        MirrorServices.RenderService.DeregisterPrePresentListener(PostPresent);
        
        _temporaryTexture?.Dispose();
        _temporaryRenderTexture?.Dispose();
        
        _temporaryTexture = null;
        _temporaryRenderTexture = null;
        
        IUIHideHandle?.Dispose();
        
        OnDispose();
    }
    
    public MappedTexture? ScreenshotTexture 
        { get; private set; }
    
    public bool ScreenshotReady 
        { get; private set; }
    
    protected virtual void OnDispose() 
        { }
    
    protected abstract void OnUIHidden();
    
    public void SetScreenshotFile(MappedTexture texture)
    {
        _temporaryTexture       = texture;
        _temporaryRenderTexture = texture.CreateRenderTarget(MirrorServices.DirectXData);
        
        MirrorServices.RenderService.RegisterPrePresentListener(PrePresent);
    }
    
    private void PostPresent()
    {
        if (!MirrorServices.UIHidingService.IsHidden)
        {
            return;
        }
        
        MirrorServices.RenderService.DeregisterPrePresentListener(PostPresent);
        
        OnUIHidden();
    }
    
    private void PrePresent()
    {
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
        
        if (_temporaryTexture == null)
        {
            return;
        }
        
        if (_temporaryRenderTexture == null)
        {
            return;
        }
        
        ShaderHandler.AlphaShader.Bind(_temporaryTexture, _temporaryRenderTexture);
        
        ShaderHandler.AlphaShader.Draw();
        
        ShaderHandler.AlphaShader.UnbindTexture();
        
        ScreenshotTexture       = _temporaryRenderTexture.ToMappedTexture(MirrorServices.DirectXData);
        
        _temporaryTexture?.Dispose();
        _temporaryRenderTexture?.Dispose();
        
        _temporaryTexture       = null;
        _temporaryRenderTexture = null;
        
        ScreenshotReady         = true;
        
        IUIHideHandle?.Dispose();
    }
}