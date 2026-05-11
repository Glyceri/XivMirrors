using System;
using System.Collections.Generic;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class RenderService : IRenderService
{
    private readonly List<Action> _renderActions = [];
    
    private readonly IMirrorLog MirrorLog;
    
    public RenderService(IMirrorLog mirrorLog) 
        => MirrorLog = mirrorLog;
    
    /// <summary>
    /// You may only call this from the native render thread c:
    /// </summary>
    public void NotifyRenderAllowed()
    {
        MirrorLog.LogExtremelyVerbose("Just Notified Render Call.");
        
        int renderSize = _renderActions.Count;
        
        for (int i = 0; i < renderSize; i++)
        {
            try
            {
                _renderActions[i].Invoke();
            }
            catch (Exception e)
            {
                MirrorLog.LogException(e);
            }
        }
    }
    
    public void RegisterRenderListener(Action renderAction)
    {
        _renderActions.Add(renderAction);
    }
    
    public void DeregisterRenderListener(Action renderAction)
    {
        _renderActions.Remove(renderAction);
    }
}