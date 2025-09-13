using FFXIVClientStructs.FFXIV.Client.Game;
using MirrorsEdge.XIVMirrors.Cameras.Interfaces;

namespace MirrorsEdge.XIVMirrors.Cameras;

internal abstract class BaseCamera : ICamera
{
    private readonly nint Address;

    public unsafe Camera* Camera => (Camera*)Address;

    public BaseCamera(nint address)
    {
        Address = address;
    }

    public abstract void Dispose();
}
