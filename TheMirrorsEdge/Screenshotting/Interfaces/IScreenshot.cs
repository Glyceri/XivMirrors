using System;

namespace TheMirrorsEdge.Services.Screenshotting.Interfaces;

public interface IScreenshot : IDisposable
{
    public Guid GUID { get; }
    
    IScreenshotFile[] ScreenshotFiles { get; }
    
    bool ScreenshotReady { get; }
}