using FFXIVClientStructs.FFXIV.Client.Game;

namespace MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;

internal unsafe class NativeCamera : BaseCamera
{
    public NativeCamera(Camera* camera) : base((nint)camera)
    {

    }

    public override void Dispose()
    {
        // Dont dispose native cameras.
    }
}
