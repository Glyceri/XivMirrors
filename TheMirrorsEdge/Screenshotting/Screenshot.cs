using System;
using TheMirrorsEdge.Services.Screenshotting.Interfaces;

namespace TheMirrorsEdge.Screenshotting;

public abstract class Screenshot : IScreenshot
{
    public IScreenshotFile[] ScreenshotFiles { get; }
    
    public Screenshot(IScreenshotFile[] screenshotFiles)
    {
        ScreenshotFiles = screenshotFiles;
    }

    public void Dispose()
    {
        foreach (IScreenshotFile file in ScreenshotFiles)
        {
            file.Dispose();
        }
    }

    public Guid GUID 
        { get; } = Guid.NewGuid();
    
    public bool ScreenshotReady
        => AreScreenshotsReady();
    
    private bool AreScreenshotsReady()
    {
        foreach (IScreenshotFile file in ScreenshotFiles)
        {
            if (file.ScreenshotReady)
            {
                continue;
            }
            
            return false;
        }
        
        return true;
    }
}