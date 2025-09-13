using MirrorsEdge.XIVMirrors.Hooking.Interfaces;
using MirrorsEdge.XIVMirrors.Services;

namespace MirrorsEdge.XIVMirrors.Hooking;

internal abstract class HookableElement : IHookableElement
{
    protected readonly DalamudServices  DalamudServices; 
    protected readonly MirrorServices   MirrorServices;

    public HookableElement(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;

        DalamudServices.Hooking.InitializeFromAttributes(this);
    }

    public abstract void Init();
    public abstract void OnDispose();

    public void Dispose()
    {
        OnDispose();
    }
}
