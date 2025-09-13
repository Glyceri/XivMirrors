using System;

namespace MirrorsEdge.XIVMirrors.Hooking.Interfaces;

internal interface IHookableElement : IDisposable
{
    public void Init();
}
