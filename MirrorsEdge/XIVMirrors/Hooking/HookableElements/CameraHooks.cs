using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using MirrorsEdge.XIVMirrors.Cameras.CameraTypes;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class CameraHooks : HookableElement
{
    private readonly RendererHook RendererHook;

    private delegate Camera* CameraManager_GetActiveCameraDelegate(CameraManager* cameraManager);
    private delegate Camera* Camera_CtorDelegate(Camera* camera);

    [Signature("E8 ?? ?? ?? ?? F7 80 84 01 00 00 FB FF FF FF", DetourName = nameof(CameraManager_GetActiveCameraDetour))]
    private readonly Hook<CameraManager_GetActiveCameraDelegate>? CameraManager_GetActiveCameraHook = null;

    [Signature("E8 ?? ?? ?? ?? EB 03 48 8B C6 45 33 C0 48 89 07", DetourName = nameof(Camera_CtorDetour))]
    private readonly Hook<Camera_CtorDelegate>? Camera_CtorHook = null;

    private MirrorCamera? OverrideCamera;

    private CameraSettings cameraSettings;

    public CameraHooks(DalamudServices dalamudServices, MirrorServices mirrorServices, RendererHook rendererHook) : base(dalamudServices, mirrorServices) 
    {
        RendererHook   = rendererHook;

        cameraSettings = new CameraSettings();

        RendererHook.RegisterRenderPassListener(OnRenderPass);
    }

    public override void Init()
    {
        CameraManager_GetActiveCameraHook?.Enable();
        Camera_CtorHook?.Enable();
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        if (Control.Instance() == null)
        {
            return;
        }

        Camera* activeCamera = Control.Instance()->CameraManager.GetActiveCamera();

        if (activeCamera == null)
        {
            return;
        }

        RenderCamera* renderCamera = activeCamera->SceneCamera.RenderCamera;

        if (renderCamera == null)
        {
            return;
        }

        cameraSettings = new CameraSettings(renderCamera);
    }

    public GameAllocation<Camera> SpawnCamera(Camera* clone = null)
    {
        GameAllocation<Camera> newCamera = new GameAllocation<Camera>();

        _ = Camera_CtorDetour(newCamera.Data);

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

    public CameraSettings GetCameraSettings()
        => cameraSettings;

    private Camera* CameraManager_GetActiveCameraDetour(CameraManager* cameraManager)
    {
        if (OverrideCamera != null)
        {
            return OverrideCamera.Camera;
        }    

        return CameraManager_GetActiveCameraHook!.Original(cameraManager);
    }

    private Camera* Camera_CtorDetour(Camera* camera)
    {
        MirrorServices.MirrorLog.Log("Camera constructor triggered");

        return Camera_CtorHook!.Original(camera);
    }

    public override void OnDispose()
    {
        RendererHook.DeregisterRenderPassListener(OnRenderPass);

        CameraManager_GetActiveCameraHook?.Dispose();
        Camera_CtorHook?.Dispose();
    }
}
