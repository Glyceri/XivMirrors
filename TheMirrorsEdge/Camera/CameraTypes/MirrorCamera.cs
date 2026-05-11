using TheMirrorsEdge.Memory;
using XIVCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;

namespace TheMirrorsEdge.Camera.CameraTypes;

public class MirrorCamera : BaseCamera
{
    private readonly GameAllocation<XIVCamera> Allocation;

    public MirrorCamera(ref GameAllocation<XIVCamera> camera) 
        : base(camera.Address)
    {
        Allocation = camera;
    }

    public override void Dispose()
    {
        Allocation.Dispose();
    }
}