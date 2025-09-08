using System;

namespace MirrorsEdge.MirrorsEdge.Windowing.Interfaces;

internal interface IMirrorWindow : IDisposable
{
    void Open();
    void Close();
    void Toggle();
}
