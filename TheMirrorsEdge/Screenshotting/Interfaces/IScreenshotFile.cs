using System;
using TheMirrorsEdge.Resources.Textures;

namespace TheMirrorsEdge.Services.Screenshotting.Interfaces;

public interface IScreenshotFile : IDisposable
{
    void SetScreenshotFile(MappedTexture texture);
    
    MappedTexture? ScreenshotTexture { get; }
    
    bool ScreenshotReady { get; }
}