using System;
using FFXIVClientStructs.FFXIV.Client.System.Memory;

namespace TheMirrorsEdge.Memory;

public class GameAllocation<T> : IDisposable 
    where T : unmanaged
{
    private bool disposed;

    internal readonly nint Address;

    internal unsafe GameAllocation(ulong align = 16)
    {
        Address = (nint)IMemorySpace.GetDefaultSpace()->Malloc<T>(align);
    }

    internal unsafe T* Data
        => (T*)Address;

    public unsafe void Dispose()
    {
        if (disposed)
        {
            return;
        }

        IMemorySpace.Free(Data);

        disposed = true;
    }
}