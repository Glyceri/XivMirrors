using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Screenshotting.ScreenshotFiles.Base;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Screenshotting.ScreenshotFiles;

public unsafe class ScreenshotFile : MappedScreenshotFile
{
    private readonly Texture* Texture;
    
    public ScreenshotFile(MirrorServices mirrorServices, ShaderHandler shaderHandler, Texture* texture)
        : base (mirrorServices, shaderHandler)
    { 
        Texture = texture;
    }

    protected override void OnUIHidden()
    {
        SetScreenshotFile(MappedTexture.CloneFrom(Texture, MirrorServices.DirectXData));
    }
}