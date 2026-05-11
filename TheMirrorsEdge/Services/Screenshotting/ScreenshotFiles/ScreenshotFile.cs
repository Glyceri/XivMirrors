using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;

namespace TheMirrorsEdge.Services.Screenshotting;

public unsafe class ScreenshotFile : IScreenshotFile
{
    public MappedTexture ScreenshotTexture { get; }

    public ScreenshotFile(DirectXData directXData, Texture* texture)
        => ScreenshotTexture = MappedTexture.CloneFrom(texture, directXData);
    
    public ScreenshotFile(DirectXData directXData, ref Texture2D texture)
        => ScreenshotTexture = new MappedTexture(directXData, ref texture);
    
    public ScreenshotFile(ref Texture2D texture, ref ShaderResourceView shaderResourceView)
        => ScreenshotTexture = new MappedTexture(ref texture, ref shaderResourceView);
    
    public bool ScreenshotReady 
        => true;
    
    public void Dispose()
        => ScreenshotTexture.Dispose();
}