using FFXIVClientStructs.FFXIV.Client.System.Memory;
using System;

namespace MirrorsEdge.MirrorsEdge.Memory;

internal class GameAllocation<T> : IDisposable where T : unmanaged
{
    private bool disposed;

    internal readonly nint Address;
    internal unsafe T* Data => (T*)Address;

    internal unsafe GameAllocation(ulong align = 16)
    {
        Address = (nint)IMemorySpace.GetDefaultSpace()->Malloc<T>(align);
    }

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
