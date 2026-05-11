using TheMirrorsEdge.Services.ChildServices;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IUIHidingService
{
    bool IsHidden { get; }
    
    IUIHideHandle HideUI();
    
    void UnhideUI();
}