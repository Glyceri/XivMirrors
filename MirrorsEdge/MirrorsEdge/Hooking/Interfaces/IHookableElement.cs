using System;

namespace MirrorsEdge.MirrorsEdge.Hooking.Interfaces;

internal interface IHookableElement : IDisposable
{
    public void Init();
}
