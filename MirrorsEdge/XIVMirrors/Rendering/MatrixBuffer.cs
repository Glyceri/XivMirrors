using MirrorsEdge.XIVMirrors.Memory;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal class MatrixBuffer : IDisposable
{
    private readonly DirectXData    DirectXData;
    private readonly DirextXBuffer  Buffer;

    public MatrixBuffer(DirectXData directXData)
    {
        DirectXData = directXData;

        Buffer      = new DirextXBuffer(DirectXData.Device, new BufferDescription()
        {
            Usage               = ResourceUsage.Dynamic,
            BindFlags           = BindFlags.ConstantBuffer,
            SizeInBytes         = Utilities.SizeOf<Matrix>(),
            CpuAccessFlags      = CpuAccessFlags.Write,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0,
        });
    }

    public void UpdateBuffer(ref Matrix matrix)
    {
        Matrix temporaryMatrix = matrix;

        DataBox box = DirectXData.Context.MapSubresource
        (
            Buffer,
            0,
            MapMode.WriteDiscard,
            MapFlags.None
        );

        Utilities.Write(box.DataPointer, ref temporaryMatrix);

        DirectXData.Context.UnmapSubresource(Buffer, 0);
    }

    public void Bind(int slot = 0)
    { 
        DirectXData.Context.VertexShader.SetConstantBuffer(slot, Buffer);
    }

    public void Dispose()
    {
        Buffer?.Dispose();
    }
}
