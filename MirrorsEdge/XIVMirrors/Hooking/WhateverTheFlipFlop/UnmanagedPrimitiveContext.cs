using static MirrorsEdge.XIVMirrors.Hooking.HookableElements.ThatShitFromKara;

namespace MirrorsEdge.XIVMirrors.Hooking.WhateverTheFlipFlop;

internal class UnmanagedPrimitiveContext(nint primitiveContext, PrimitiveContextDrawCommand drawCommand)
{
    // commandType:
    // 17 = wire
    // 33 = bad quad
    // 35 = also quad?

    public nint DrawCommand(ulong commandType, uint vertices, uint priority, nint material)
    {
        return drawCommand(primitiveContext, commandType, vertices, priority, material);
    }
}
