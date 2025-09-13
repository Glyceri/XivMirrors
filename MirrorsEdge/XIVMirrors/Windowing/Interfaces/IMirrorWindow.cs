using System;

namespace MirrorsEdge.XIVMirrors.Windowing.Interfaces;

internal interface IMirrorWindow : IDisposable
{
    void Open();
    void Close();
    void Toggle();
}
