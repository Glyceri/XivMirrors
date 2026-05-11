using System;
using TheMirrorsEdge.Resources.Textures;

namespace TheMirrorsEdge.Services.Screenshotting.Interfaces;

public interface IScreenshotFile : IDisposable
{
    MappedTexture? ScreenshotTexture { get; }
    
    bool ScreenshotReady { get; }
}