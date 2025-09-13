using FFXIVClientStructs.FFXIV.Client.Game;

namespace MirrorsEdge.XIVMirrors.Cameras.CameraTypes;

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
