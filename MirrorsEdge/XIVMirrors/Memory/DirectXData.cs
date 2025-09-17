using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using KernelDeviceObject = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;
using KernelSwapChain = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.SwapChain;

namespace MirrorsEdge.XIVMirrors.Memory;

internal unsafe class DirectXData : IDisposable
{
    public readonly Device              Device;
    public readonly SwapChain           SwapChain;
    public readonly DeviceContext       Context;
    public readonly KernelSwapChain*    KernelSwapChain;
    public readonly KernelDeviceObject* KernelDevice;

    public DirectXData(MirrorServices mirrorServices)
    {
        // These can throw... but if it happens the plugin shouldn't be allowed to run to begin with, so I am not chatching it.

        KernelDevice = KernelDeviceObject.Instance();

        SwapChain       = new SwapChain((nint)KernelDevice->SwapChain->DXGISwapChain);
        KernelSwapChain = KernelDevice->SwapChain;
        Device          = SwapChain.GetDevice<Device>();
        Context         = Device.ImmediateContext;
    }

    public void Dispose()
    {
        Device?.Dispose();
    }
}
