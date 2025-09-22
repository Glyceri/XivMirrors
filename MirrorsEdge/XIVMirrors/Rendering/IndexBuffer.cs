using SharpDX.Direct3D11;
using System;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal unsafe class IndexBuffer : IDisposable
{
    public readonly ushort[]        Incides;
    public readonly DirextXBuffer   Buffer;

    public IndexBuffer(ref ushort[] indices, ref DirextXBuffer buffer)
    {
        Incides = indices;
        Buffer  = buffer;
    }

    public int IndicesCount
        => Incides.Length;

    public void Dispose()
    {
        Buffer.Dispose();
    }
}
