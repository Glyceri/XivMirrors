using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;

namespace TheMirrorsEdge.Services.Screenshotting;

public class ViewportScreenshotFile : IScreenshotFile
{
    public MappedTexture? ScreenshotTexture 
        { get; private set; } = null;
    
    public bool ScreenshotReady 
        { get; private set; } = false;
    
    private readonly MirrorServices             MirrorServices;
    private readonly DalamudServices            DalamudServices;
    private readonly CancellationTokenSource    CancellationTokenSource;
    
    private Task<IDalamudTextureWrap>? TextureWrapTask;
    private IDalamudTextureWrap?       TextureWrap;
    
    private static readonly ImGuiViewportTextureArgs TextureArguments = new ImGuiViewportTextureArgs()
    {
        AutoUpdate              = true,
        KeepTransparency        = false,
        TakeBeforeImGuiRender   = true,
        ViewportId              = ImGui.GetMainViewport().ID,
    };
    
    public ViewportScreenshotFile(DalamudServices dalamudServices, MirrorServices mirrorServices)
    { 
        CancellationTokenSource = new CancellationTokenSource();
        
        MirrorServices  = mirrorServices;
        DalamudServices = dalamudServices;
        
        MirrorServices.RenderService.RegisterPrePresentListener(PrePresent);
        
        TextureWrapTask = CreateTextureWrap();
    }
    
    private Task<IDalamudTextureWrap> CreateTextureWrap()
    {
        MirrorServices.MirrorLog.Log("Created Dalamud Viewport Texture Wrap.");

        ImGuiViewportTextureArgs textureArguments = new ImGuiViewportTextureArgs()
        {
            AutoUpdate              = false,
            KeepTransparency        = false,
            TakeBeforeImGuiRender   = true,
            ViewportId              = ImGui.GetMainViewport().ID,
        };
        
        return DalamudServices.TextureProvider.CreateFromImGuiViewportAsync(textureArguments, cancellationToken: CancellationTokenSource.Token);
    }
    
    private void PrePresent()
    {
        if (TextureWrapTask == null)
        {
            return;
        }
        
        if (!TextureWrapTask.IsCompleted && !TextureWrapTask.IsCanceled && !TextureWrapTask.IsFaulted)
        {
            return;
        }
        
        MirrorServices.MirrorLog.LogVerbose("Texture Wrap Task is completed.");
        
        TextureWrap = TextureWrapTask.Result;
        
        try
        {
            ShaderResourceView shaderResourceView = new ShaderResourceView((nint)TextureWrap.Handle.Handle);
            
            Texture2D?         texture2D          = shaderResourceView.Resource.QueryInterface<Texture2D>();
            
            if (texture2D != null)
            {
                ScreenshotTexture = new MappedTexture(ref texture2D, ref shaderResourceView);
                ScreenshotReady   = true;
            }
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
        
        TextureWrapTask?.Dispose();
        TextureWrapTask = null;
        
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
    }
    
    public void Dispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
     
        TextureWrap?.Dispose();
        TextureWrapTask?.Dispose();
        
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
        
        ScreenshotTexture?.Dispose();
    }
}