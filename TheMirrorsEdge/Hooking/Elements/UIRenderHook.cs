using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Hooking.Elements;

public class UIRenderHook : HookableElement
{
    private delegate nint PushbackUIDelegate(nint a1, char a2);
    
    [Signature("E8 ?? ?? ?? ?? EB ?? E8 ?? ?? ?? ?? 4C 8D 5C 24 50", DetourName = nameof(PushbackUIDetour))]
    private Hook<PushbackUIDelegate>? PushbackUIHook = null;
    
    public UIRenderHook(DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(dalamudServices, mirrorServices) { }

    public override void Init()
    {
        //PushbackUIHook?.Enable();
    }

    public override void Dispose()
    {
        PushbackUIHook?.Dispose();
    }
    
    private nint PushbackUIDetour(nint a1, char a2)
    {
        MirrorServices.MirrorLog.LogVerbose($"Pushing back UI! [{a1}, {(int)a2}]");
        
        return PushbackUIHook!.Original(a1, a2);
    }
}