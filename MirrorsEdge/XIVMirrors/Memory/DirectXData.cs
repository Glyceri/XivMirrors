using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using KernelDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

namespace MirrorsEdge.XIVMirrors.Memory;

internal unsafe class DirectXData : IDisposable
{
    public readonly Device          Device;
    public readonly SwapChain       SwapChain;
    public readonly DeviceContext   Context;

    public DirectXData(MirrorServices mirrorServices)
    {
        // These can throw... but if it happens the plugin shouldn't be allowed to run to begin with, so I am not chatching it.

        KernelDevice* kernelDevice = KernelDevice.Instance();

        SwapChain   = new SwapChain((nint)kernelDevice->SwapChain->DXGISwapChain);
        Device      = SwapChain.GetDevice<Device>();
        Context     = Device.ImmediateContext;
    }

    public void Dispose()
    {
        Device?.Dispose();

        // TODO: should context be disposed?
    }
}
