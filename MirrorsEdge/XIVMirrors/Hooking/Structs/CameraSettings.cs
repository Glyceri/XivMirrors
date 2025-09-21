using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace MirrorsEdge.XIVMirrors.Hooking.Structs;

internal unsafe readonly struct CameraSettings
{
    public readonly Matrix4x4   ViewMatrix;
    public readonly Matrix4x4   ProjectionMatrix2;
    public readonly Vector3     Origin;
    public readonly Matrix4x4   ProjectionMatrix;
    public readonly float       FoV;
    public readonly float       FoV_2;
    public readonly float       AspectRatio;
    public readonly float       NearPlane;
    public readonly float       FarPlane;
    public readonly float       OrthoHeight;
    public readonly bool        IsOrtho;
    public readonly bool        StandardZ;
    public readonly bool        FiniteFarPlane;

    public CameraSettings()
    {
        ViewMatrix          = Matrix4x4.Identity;
        ProjectionMatrix2   = Matrix4x4.Identity;
        Origin              = Vector3.Zero;
        ProjectionMatrix    = Matrix4x4.Identity;
        FoV                 = 90;
        FoV_2               = 90;
        AspectRatio         = 16.0f / 9.0f;
        NearPlane           = 0.1f;
        FarPlane            = 1000.0f;
        OrthoHeight         = 5f;
        IsOrtho             = false;
        StandardZ           = true;
        FiniteFarPlane      = true;
    }

    public CameraSettings(Camera* camera)
    {
        ViewMatrix          = camera->ViewMatrix; 
        ProjectionMatrix    = camera->ProjectionMatrix;
        Origin              = camera->Origin;
        FoV                 = camera->FoV;
        FoV_2               = camera->FoV_2;
        AspectRatio         = camera->AspectRatio;
        NearPlane           = camera->NearPlane;
        FarPlane            = camera->FarPlane;
        OrthoHeight         = camera->OrthoHeight;
        IsOrtho             = camera->IsOrtho;
        StandardZ           = camera->StandardZ;
        FiniteFarPlane      = camera->FiniteFarPlane;
    }
}
