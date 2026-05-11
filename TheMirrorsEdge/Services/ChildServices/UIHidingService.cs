using System;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public unsafe class UIHidingService : IUIHidingService
{
    private bool _lastUIHideState = false;
    private bool _isInUIHide      = false;
    
    private int  _hideCalls       = 0;
    
    private readonly IMirrorLog MirrorLog;
    
    public UIHidingService(IMirrorLog mirrorLog)
        => MirrorLog = mirrorLog;
    
    public bool IsHidden 
        => IsUIHidden();
    
    private bool IsUIHidden()
    {
        RaptureAtkModule* raptureAtkModule = Framework.Instance()->GetUIModule()->GetRaptureAtkModule();
        
        if (raptureAtkModule == null)
        {
            return true;
        }
        
        return raptureAtkModule->RaptureAtkUnitManager.Flags.HasFlag(AtkUnitManagerFlags.UiHidden);
    }
    
    
    // DOES NOT WORK OPROPERLY. CHECK TOMORROW IF YOU CAN HOOK THAT UI DETOUR AND SEE IF IT HAS A SEPERATE CALL FOR HIDDEN UI!
    public IUIHideHandle HideUI()
    {
        _hideCalls++;
        
        if (_isInUIHide)
        {
            return new UIHideHandle(this);
        }
        
        RaptureAtkModule* raptureAtkModule = Framework.Instance()->GetUIModule()->GetRaptureAtkModule();
        
        if (raptureAtkModule == null)
        {
            return new UIHideHandle(this);
        }
        
        _lastUIHideState = raptureAtkModule->RaptureAtkUnitManager.Flags.HasFlag(AtkUnitManagerFlags.UiHidden);
        _isInUIHide      = true;
        
        MirrorLog.LogVerbose($"UI HIDE: {_lastUIHideState}, {_isInUIHide}.");
        
        raptureAtkModule->SetUiVisibility(false);
        
        return new UIHideHandle(this);
    }
    
    public void UnhideUI()
    {
        if (_hideCalls <= 0)
        {
            return;
        }
        
        _hideCalls--;
        
        if (_hideCalls > 0)
        {
            return;
        }
        
        if (!_isInUIHide)
        {
            return;
        }
        
        _isInUIHide = false;
        
        MirrorLog.LogVerbose($"UI UNHIDE: {_lastUIHideState}, {_isInUIHide}.");
        
        if (_lastUIHideState)
        {
            return;
        }
        
        RaptureAtkModule* raptureAtkModule = Framework.Instance()->GetUIModule()->GetRaptureAtkModule();
        
        if (raptureAtkModule == null)
        {
            return;
        }
        
        if (!raptureAtkModule->RaptureAtkUnitManager.Flags.HasFlag(AtkUnitManagerFlags.UiHidden))
        {
            return;
        }
        
        raptureAtkModule->SetUiVisibility(true);
    }
    
    private class UIHideHandle : IUIHideHandle
    {
        private bool DISPOSED = false;
        
        private readonly UIHidingService UIHidingService;
        
        public UIHideHandle(UIHidingService uiHidingService)
            => UIHidingService = uiHidingService;
        
        public void Dispose()
        {
            if (DISPOSED)
            {
                return;
            }
            
            DISPOSED = true;
            
            UIHidingService.UnhideUI();
        }
    }
}

public interface IUIHideHandle : IDisposable;