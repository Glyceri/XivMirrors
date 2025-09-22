using MirrorsEdge.XIVMirrors.Memory;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Runtime.InteropServices;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal class BasicModel : IDisposable
{
    private readonly DirectXData  DirectXData;

    private readonly VertexBuffer VertexBuffer;
    private readonly IndexBuffer  IndicesBuffer;

    public BasicModel(DirectXData directXData, ref (Vertex[] vertices, ushort[] indices) model)
    {
        DirectXData   = directXData;

        VertexBuffer  = CreateVertexBuffer(ref model.vertices);
        IndicesBuffer = CreateIndexBuffer(ref model.indices);
    }

    public void BindBuffer(int slot = 0)
    {
        DirectXData.Context.InputAssembler.SetVertexBuffers(slot, VertexBuffer.Buffer);

        DirectXData.Context.InputAssembler.SetIndexBuffer(IndicesBuffer.Buffer, Format.R16_UInt, 0);

        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
    }

    public void Draw()
    {
        DirectXData.Context.DrawIndexed(IndicesBuffer.IndicesCount, 0, 0);
    }

    private VertexBuffer CreateVertexBuffer(ref Vertex[] vertices)
    {
        Span<Vertex>    vertexSpan  = new Span<Vertex>(vertices);
        Span<byte>      byteBuffer  = MemoryMarshal.AsBytes(vertexSpan);

        DirextXBuffer   buffer      = CreateBuffer(byteBuffer, BindFlags.VertexBuffer);

        return new VertexBuffer(ref vertices, ref buffer);
    }

    private IndexBuffer CreateIndexBuffer(ref ushort[] indices)
    {
        Span<ushort>    indicesSpan = new Span<ushort>(indices);
        Span<byte>      byteBuffer  = MemoryMarshal.AsBytes(indicesSpan);

        DirextXBuffer   buffer      = CreateBuffer(byteBuffer, BindFlags.IndexBuffer);

        return new IndexBuffer(ref indices, ref buffer);
    }

    private DirextXBuffer CreateBuffer(Span<byte> bytes, BindFlags bindFlags)
    {
        return DirextXBuffer.Create(DirectXData.Device, bytes.ToArray(), new BufferDescription()
        {
            Usage               = ResourceUsage.Immutable,
            BindFlags           = bindFlags,
            SizeInBytes         = bytes.Length,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0,
        });
    }

    public void Dispose()
    {
        VertexBuffer?.Dispose();
        IndicesBuffer?.Dispose();
    }
}
