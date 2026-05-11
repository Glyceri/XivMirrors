using System;
using SharpDX;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Memory;

using ConstantBuffer = SharpDX.Direct3D11.Buffer;

namespace TheMirrorsEdge.Resources;

public abstract class BasicBuffer<T> : IDisposable 
    where T : struct
{
    protected readonly bool        IsNative;
    protected readonly DirectXData DirectXData;

    public ConstantBuffer ConstantBuffer { get; private init; }

    public BasicBuffer(DirectXData directXData)
    {
        DirectXData = directXData;

        IsNative    = false;

        BufferDescription bufferDesc = new BufferDescription()
        {
            SizeInBytes         = Utilities.SizeOf<T>(),
            Usage               = ResourceUsage.Dynamic,
            BindFlags           = BindFlags.ConstantBuffer,
            CpuAccessFlags      = CpuAccessFlags.Write,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0
        };

        ConstantBuffer = new ConstantBuffer(DirectXData.Device, bufferDesc);
    }

    public BasicBuffer(DirectXData directXData, nint basicBufferPointer)
    {
        DirectXData     = directXData;

        IsNative        = true;

        ConstantBuffer  = new ConstantBuffer(basicBufferPointer);
    }

    public virtual void UpdateConstantBuffer(ref T dataObject)
    {
        DataBox box = DirectXData.Context.MapSubresource(ConstantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);

        Utilities.Write(box.DataPointer, ref dataObject);

        DirectXData.Context.UnmapSubresource(ConstantBuffer, 0);
    }

    public virtual void UpdateBuffer(T dataObject)
        => UpdateConstantBuffer(ref dataObject);

    public virtual void BindToVertexShader(uint slot = 0)
        => DirectXData.Context.VertexShader.SetConstantBuffer((int)slot, ConstantBuffer);

    public virtual void BindToFragmentShader(uint slot = 0)
        => DirectXData.Context.PixelShader.SetConstantBuffer((int)slot, ConstantBuffer);

    protected virtual void OnDispose() { }

    public void Dispose()
    {
        OnDispose();

        if (IsNative)
        {
            return;
        }    

        ConstantBuffer?.Dispose();
    }
}