using SharpDX.Direct3D11;
using System;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal unsafe class VertexBuffer : IDisposable
{
    public readonly Vertex[]              Vertices;
    public readonly VertexBufferBinding   Buffer;

    public VertexBuffer(ref Vertex[] vertices, ref DirextXBuffer buffer)
    {
        Vertices    = vertices;
        Buffer      = new VertexBufferBinding(buffer, 0, sizeof(Vertex));
    }

    public int VertexCount => Vertices.Length;

    public void Dispose()
    {
        Buffer.Buffer.Dispose();
    }
}
