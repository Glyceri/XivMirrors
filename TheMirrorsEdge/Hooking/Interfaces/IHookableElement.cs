using System;

namespace TheMirrorsEdge.Hooking.Interfaces;

public interface IHookableElement : IDisposable
{
    void Init();
}