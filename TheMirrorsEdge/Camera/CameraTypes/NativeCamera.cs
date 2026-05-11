using TheMirrorsEdge.Camera.Enums;
using XIVCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;

namespace TheMirrorsEdge.Camera.CameraTypes;

public unsafe class NativeCamera : BaseCamera
{
    public readonly NativeCameraType NativeCameraType;
    
    public NativeCamera(XIVCamera* camera, NativeCameraType nativeCameraType) 
        : base((nint)camera)
    {
        NativeCameraType = nativeCameraType;
    }

    public override void Dispose()
        { }
}