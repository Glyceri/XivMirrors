using FFXIVClientStructs.FFXIV.Client.Game;
using MirrorsEdge.MirrorsEdge.Memory;

namespace MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;

internal unsafe class MirrorCamera : BaseCamera
{
    private readonly GameAllocation<Camera> Allocation;

    public MirrorCamera(ref GameAllocation<Camera> camera) : base(camera.Address)
    {
        Allocation = camera;
    }

    public override void Dispose()
    {
        Allocation.Dispose();
    }
}
