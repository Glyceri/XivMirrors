using XIVCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;

namespace TheMirrorsEdge.Camera.CameraTypes;

public unsafe class NativeCamera : BaseCamera
{
    public NativeCamera(XIVCamera* camera) 
        : base((nint)camera) { }

    public override void Dispose()
        { }
}