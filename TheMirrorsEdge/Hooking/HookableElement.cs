using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using TheMirrorsEdge.Hooking.Interfaces;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Hooking;

public abstract class HookableElement : IHookableElement
{
    protected readonly DalamudServices DalamudServices;
    protected readonly MirrorServices  MirrorServices;
    
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

    protected Hook<T> GetHook<T>(nint baseAddress, uint vTableOffset, uint vTableIndex, T detour) 
        where T : Delegate
            => DalamudServices.Hooking.HookFromAddress<T>(GetVTableAddress(GetVTable(baseAddress, vTableOffset), vTableIndex), detour);
    
    public abstract void Init();
    public abstract void Dispose();
}