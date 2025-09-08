using FFXIVClientStructs.FFXIV.Client.Game;
using MirrorsEdge.MirrorsEdge.Cameras.Interfaces;

namespace MirrorsEdge.MirrorsEdge.Cameras;

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
