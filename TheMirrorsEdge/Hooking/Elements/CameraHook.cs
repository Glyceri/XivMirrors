using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using SharpDX;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Services;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;

namespace TheMirrorsEdge.Hooking.Elements;

public unsafe class CameraHook : HookableElement
{
    private delegate Camera* CameraManager_GetActiveCameraDelegate(CameraManager* cameraManager);
    private delegate Camera* Camera_CtorDelegate(Camera* camera);

    [Signature("E8 ?? ?? ?? ?? F7 80 84 01 00 00 FB FF FF FF", DetourName = nameof(CameraManager_GetActiveCameraDetour))]
    private readonly Hook<CameraManager_GetActiveCameraDelegate>? CameraManager_GetActiveCameraHook = null;

    [Signature("E8 ?? ?? ?? ?? EB 03 48 8B C6 45 33 C0 48 89 07", DetourName = nameof(Camera_CtorDetour))]
    private readonly Hook<Camera_CtorDelegate>? Camera_CtorHook = null;

    //private MirrorCamera? OverrideCamera;

    private delegate nint GetEngineCoreSingletonDelegate();

    private Matrix ViewProjMatrix   = Matrix.Identity;
    private Matrix ViewMatrix       = Matrix.Identity;
    private Matrix ProjectionMatrix = Matrix.Identity;
    private float  NearPlane        = 0;
    private float  FarPlane         = 0;

    private delegate nint EnvironmentManagerUpdate(nint thisPtr, nint unk1);

    [Signature("48 89 5C 24 ?? 55 56 57 41 55 41 56 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05", DetourName = nameof(EnvironmentManagerUpdateDetour))]
    private readonly Hook<EnvironmentManagerUpdate>? EnvironmentManagerUpdateHook = null!;

    public CameraHook(DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(dalamudServices, mirrorServices) { }

    public override void Init()
    {
        CameraManager_GetActiveCameraHook?.Enable();
        Camera_CtorHook?.Enable();

        EnvironmentManagerUpdateHook?.Enable();
    }

    private nint EnvironmentManagerUpdateDetour(nint thisPtr, nint unk1)
    {
        OnRenderPass();

        return EnvironmentManagerUpdateHook!.Original(thisPtr, unk1);
    }

    private void OnRenderPass()
    {
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

        ViewMatrix           = new Matrix(renderCamera->ViewMatrix.Matrix.ToArray());
        ViewMatrix.M44       = 1; // Inverse it
        ProjectionMatrix     = new Matrix(renderCamera->ProjectionMatrix.Matrix.ToArray());
        ViewProjMatrix       = ViewMatrix * ProjectionMatrix;
        NearPlane            = renderCamera->NearPlane;
        FarPlane             = renderCamera->FarPlane;
    }

    public Matrix GetViewMatrix()
        => ViewMatrix;

    public Matrix GetProjectionMatrix()
        => ProjectionMatrix;

    public Matrix GetViewProjectionMatrix()
        => ViewProjMatrix;

    public float GetNearPlane()
        => NearPlane;

    public float GetFarPlane()
        => FarPlane;

    public CameraBufferLayout GetCameraBufferLayout(Matrix modelMatrix)
        => new CameraBufferLayout(modelMatrix, ViewMatrix, ProjectionMatrix, NearPlane, FarPlane);

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

    /*
    public void SetOverride(MirrorCamera? overrideCamera)
    {
        OverrideCamera = overrideCamera;
    }
    */

    private Camera* CameraManager_GetActiveCameraDetour(CameraManager* cameraManager)
    {
        /*
        if (OverrideCamera != null)
        {
            return OverrideCamera.Camera;
        }    
    */

        return CameraManager_GetActiveCameraHook!.Original(cameraManager);
    }

    private Camera* Camera_CtorDetour(Camera* camera)
    {
        MirrorServices.MirrorLog.Log("Camera constructor triggered");

        return Camera_CtorHook!.Original(camera);
    }

    public override void Dispose()
    {
        EnvironmentManagerUpdateHook?.Dispose();

        CameraManager_GetActiveCameraHook?.Dispose();
        Camera_CtorHook?.Dispose();
    }
}
