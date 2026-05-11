using System;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IRenderService
{
    void NotifyRenderAllowed();
    void NotifyPrePresent();
    void NotifyPostPresent();
    
    void RegisterRenderListener(Action renderAction);
    void DeregisterRenderListener(Action renderAction);
    
    void RegisterPrePresentListener(Action presentAction);
    void DeregisterPrePresentListener(Action presentAction);
    
    void RegisterPostPresentListener(Action presentAction);
    void DeregisterPostPresentListener(Action presentAction);
}