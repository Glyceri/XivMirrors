using Dalamud.Bindings.ImGui;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources.Struct;
using SharpDX;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Resources;

internal abstract class BasicTexture : IDisposable
{
    private uint lastWidth  = 0;
    private uint lastHeight = 0;

    private uint lastActualWidth    = 0;
    private uint lastActualHeight   = 0;

    public readonly SharpDX.Direct3D11.Buffer ConstantBuffer;

    public uint Width   { get; protected set; }
    public uint Height  { get; protected set; }

    public abstract Texture2D           Texture             { get; }
    public abstract ShaderResourceView  ShaderResourceView  { get; }

    public abstract uint  ActualWidth   { get; }
    public abstract uint  ActualHeight  { get; }

    public abstract nint         TextureHandle  { get; }
    public abstract ImTextureID  Handle         { get; }

    public abstract ScaledResolution  ScaledResolution { get; }

    public BasicTexture(DirectXData directXData)
    {
        BufferDescription bufferDesc = new BufferDescription()
        {
            SizeInBytes         = Utilities.SizeOf<ScaledResolution>(),
            Usage               = ResourceUsage.Dynamic,
            BindFlags           = BindFlags.ConstantBuffer,
            CpuAccessFlags      = CpuAccessFlags.Write,
            OptionFlags         = ResourceOptionFlags.None,
            StructureByteStride = 0
        };

        ConstantBuffer = new SharpDX.Direct3D11.Buffer(directXData.Device, bufferDesc);
    }

    public void UpdateConstantBuffer(DirectXData directXData)
    {
        bool changed = false;

        if (lastWidth != Width)
        {
            lastWidth = Width;

            changed = true;
        }

        if (lastHeight != Height)
        {
            lastHeight = Height;

            changed = true;
        }

        if (lastActualWidth != ActualWidth)
        {
            lastActualWidth = ActualWidth;

            changed = true;
        }

        if (lastActualHeight != ActualHeight)
        {
            lastActualHeight = ActualHeight;

            changed = true;
        }

        if (!changed)
        {
            return;
        }

        ScaledResolution scaledResolution = ScaledResolution;

        DataBox box = directXData.Context.MapSubresource(
            ConstantBuffer,
            0,
            MapMode.WriteDiscard,
            MapFlags.None);

        Utilities.Write(box.DataPointer, ref scaledResolution);

        directXData.Context.UnmapSubresource(ConstantBuffer, 0);
    }

    public virtual void Dispose()
    {
        ConstantBuffer?.Dispose();
    }
}
