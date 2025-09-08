using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;
using MirrorsEdge.MirrorsEdge.Memory;
using MirrorsEdge.MirrorsEdge.Services;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class CameraHooks : HookableElement
{
    private Camera myCamera = new Camera();

    private delegate Camera* CameraManager_GetActiveCameraDelegate(CameraManager* cameraManager);
    private delegate Camera* Camera_CtorDelegate(Camera* camera);

    [Signature("E8 ?? ?? ?? ?? F7 80 84 01 00 00 FB FF FF FF", DetourName = nameof(CameraManager_GetActiveCameraDetour))]
    private readonly Hook<CameraManager_GetActiveCameraDelegate>? CameraManager_GetActiveCameraHook = null;

    [Signature("E8 ?? ?? ?? ?? EB 03 48 8B C6 45 33 C0 48 89 07", DetourName = nameof(Camera_CtorDetour))]
    private readonly Hook<Camera_CtorDelegate>? Camera_CtorHook = null;

    private MirrorCamera? OverrideCamera;

    public CameraHooks(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices) 
    {
        
    }

    public GameAllocation<Camera> SpawnCamera(Camera* clone = null)
    {
        GameAllocation<Camera> newCamera = new GameAllocation<Camera>();

        Camera_CtorDetour(newCamera.Data);

        if (clone != null)
        {
            *newCamera.Data = *clone;
        }

        return newCamera;
    }

    public void SetOverride(MirrorCamera? overrideCamera)
    {
        OverrideCamera = overrideCamera;
    }

    public override void Init()
    {
        CameraManager_GetActiveCameraHook?.Enable();
        Camera_CtorHook?.Enable();
    }

    private Camera* CameraManager_GetActiveCameraDetour(CameraManager* cameraManager)
    {
        if (OverrideCamera != null)
        {
            return OverrideCamera.Camera;
        }    

        //MirrorServices.MirrorLog.Log("Get Active Camera!");

        return CameraManager_GetActiveCameraHook!.OriginalDisposeSafe(cameraManager);
    }

    private Camera* Camera_CtorDetour(Camera* camera)
    {
        MirrorServices.MirrorLog.Log("Camera constructor triggered");

        return Camera_CtorHook!.OriginalDisposeSafe(camera);
    }

    public override void OnDispose()
    {
        CameraManager_GetActiveCameraHook?.Dispose();
        Camera_CtorHook?.Dispose();
    }
}
