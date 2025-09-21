using MirrorsEdge.XIVMirrors.Hooking.Interfaces;
using MirrorsEdge.XIVMirrors.Services;
using System.Runtime.InteropServices;

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

    protected nint GetVTable(nint address, uint offset = 0)
        => Marshal.ReadIntPtr(address, (int)offset * nint.Size);

    protected nint GetVTableAddress(nint vtable, uint index)
        => Marshal.ReadIntPtr(vtable, (int)index * nint.Size);

    public abstract void Init();
    public abstract void OnDispose();

    public void Dispose()
    {
        OnDispose();
    }
}
