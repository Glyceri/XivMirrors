using System;
using System.Collections.Generic;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class RenderService : IRenderService
{
    private readonly List<Action> _renderActions     = [];
    private readonly List<Action> _preRenderActions  = [];
    private readonly List<Action> _postRenderActions = [];
    
    private readonly IMirrorLog MirrorLog;
    
    public RenderService(IMirrorLog mirrorLog) 
        => MirrorLog = mirrorLog;
    
    private void CallActions(in List<Action> actions)
    {
        int renderSize = actions.Count;
        
        for (int i = renderSize - 1; i >= 0; i--)
        {
            try
            {
                actions[i].Invoke();
            }
            catch (Exception e)
            {
                MirrorLog.LogException(e);
            }
        }
    }
    
    /// <summary>
    /// You may only call this from the native render thread c:
    /// </summary>
    public void NotifyRenderAllowed()
    {
        MirrorLog.LogExtremelyVerbose("Just Notified Render Call.");
        
        CallActions(in _renderActions);
    }
    
    public void NotifyPrePresent()
    {
        MirrorLog.LogExtremelyVerbose("Just Notified Pre Present Call.");
        
        CallActions(in _preRenderActions);
    }
    
    public void NotifyPostPresent()
    {
        MirrorLog.LogExtremelyVerbose("Just Notified Post Present Call.");
        
        CallActions(in _postRenderActions);
    }
    
    public void RegisterRenderListener(Action renderAction)
        => _renderActions.Insert(0, renderAction);

    public void DeregisterRenderListener(Action renderAction)
        => _renderActions.Remove(renderAction);
    
    public void RegisterPrePresentListener(Action presentAction)
        => _preRenderActions.Insert(0, presentAction);

    public void DeregisterPrePresentListener(Action presentAction)
        => _preRenderActions.Remove(presentAction);
    
    public void RegisterPostPresentListener(Action presentAction)
        => _postRenderActions.Insert(0, presentAction);

    public void DeregisterPostPresentListener(Action presentAction)
        => _postRenderActions.Remove(presentAction);
}