using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Mirrors;

internal class Mirror : IDisposable
{
    private readonly VertexBuffer   SquareBuffer;
    private readonly DirectXData    DirectXData;

    public Mirror(DirectXData directXData)
    {
        DirectXData     = directXData;
        SquareBuffer    = CreateVertexBuffer(PrimitiveFactory.Quad());
    }

    public VertexBuffer CreateVertexBuffer(IEnumerable<Vertex> vertices)
    {
        Vertex[]        vertexArray  = vertices.ToArray();
        Span<Vertex>    vertexSpan   = new Span<Vertex>(vertexArray);
        Span<byte>      byteBuffer   = MemoryMarshal.AsBytes(vertexSpan);

        DirextXBuffer   buffer       = CreateBuffer(byteBuffer, BindFlags.VertexBuffer);

        return new VertexBuffer(ref vertexArray, ref buffer);
    }

    public void Draw()
    {
        DirectXData.Context.InputAssembler.SetVertexBuffers(0, SquareBuffer.Buffer);

        DirectXData.Context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

        DirectXData.Context.Draw(SquareBuffer.VertexCount, 0);
    }

    public DirextXBuffer CreateBuffer(Span<byte> bytes, BindFlags bindFlags)
    {
        return new DirextXBuffer(DirectXData.Device, new BufferDescription()
        {
            Usage               = ResourceUsage.Dynamic,
            BindFlags           = bindFlags,
            SizeInBytes         = bytes.Length,
            CpuAccessFlags      = CpuAccessFlags.Write,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0,
        });
    }

    public void Dispose()
    {
        SquareBuffer.Dispose();
    }
}
