using System;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Screenshotting.ScreenshotFiles.Base;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Screenshotting.ScreenshotFiles;

public class ViewportScreenshotFile : MappedScreenshotFile
{
    private readonly DalamudServices            DalamudServices;
    private readonly CancellationTokenSource    CancellationTokenSource;
    
    private Task<IDalamudTextureWrap>?          TextureWrapTask;
    private IDalamudTextureWrap?                TextureWrap;
    
    public ViewportScreenshotFile(MirrorServices mirrorServices, ShaderHandler shaderHandler, DalamudServices dalamudServices)
        : base (mirrorServices, shaderHandler)
    { 
        DalamudServices         = dalamudServices;
        CancellationTokenSource = new CancellationTokenSource();
    }

    protected override void OnUIHidden()
    {
        TextureWrapTask = CreateTextureWrap();
        
        MirrorServices.RenderService.RegisterPrePresentListener(PrePresent);
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
                SetScreenshotFile(new MappedTexture(ref texture2D, ref shaderResourceView));
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
    
    protected override void OnDispose()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
     
        TextureWrap?.Dispose();
        TextureWrapTask?.Dispose();
        
        MirrorServices.RenderService.DeregisterPrePresentListener(PrePresent);
    }
}