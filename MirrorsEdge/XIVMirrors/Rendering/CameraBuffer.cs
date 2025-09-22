using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering.Structs;
using SharpDX;
using SharpDX.Direct3D11;
using System;
using DirextXBuffer = SharpDX.Direct3D11.Buffer;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal class CameraBuffer : IDisposable
{
    private readonly DirectXData    DirectXData;
    private readonly DirextXBuffer  Buffer;

    public CameraBuffer(DirectXData directXData)
    {
        DirectXData = directXData;

        Buffer                  = new DirextXBuffer(DirectXData.Device, new BufferDescription()
        {
            Usage               = ResourceUsage.Dynamic,
            BindFlags           = BindFlags.ConstantBuffer,
            SizeInBytes         = Utilities.SizeOf<CameraBufferLayout>(),
            CpuAccessFlags      = CpuAccessFlags.Write,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0,
        });
    }

    public void UpdateBuffer(ref CameraBufferLayout cameraBufferLayout)
    {
        CameraBufferLayout temporaryMatrix = cameraBufferLayout;

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
