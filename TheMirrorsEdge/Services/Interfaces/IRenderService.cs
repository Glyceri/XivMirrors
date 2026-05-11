using System;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IRenderService
{
    void NotifyRenderAllowed();
    
    void RegisterRenderListener(Action renderAction);
    void DeregisterRenderListener(Action renderAction);
}