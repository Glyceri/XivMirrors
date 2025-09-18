using static MirrorsEdge.XIVMirrors.Hooking.HookableElements.ThatShitFromKara;

namespace MirrorsEdge.XIVMirrors.Hooking.WhateverTheFlipFlop;

internal class UnmanagedPrimitiveContext(nint data, PrimitiveContextDrawCommand drawCommand)
{
    public nint DrawCommand(ulong commandType, uint vertices, uint priority, nint material)
    {
        return drawCommand(data, commandType, vertices, priority, material);
    }
}
