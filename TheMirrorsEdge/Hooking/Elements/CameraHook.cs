using System.Runtime.InteropServices;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Common.Math;
using SharpDX;
using TheMirrorsEdge.Camera.CameraTypes;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Services;

using Vector3         = SharpDX.Vector3;
using XIVCamera       = FFXIVClientStructs.FFXIV.Client.Game.Camera;
using XIVSceneCamera  = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;
using XIVRenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;

namespace TheMirrorsEdge.Hooking.Elements;

// https://github.com/Etheirys/Brio/blob/5dd67b887a2fcf199628d4b793c4e8ef4dcd46f6/Brio/Game/Camera/CameraService.cs#L21

// DO it like this dumbass

public unsafe class CameraHook : HookableElement
{
    private delegate XIVCamera* CameraManager_GetActiveCameraDelegate(CameraManager* cameraManager);
    private delegate XIVCamera* Camera_CtorDelegate(XIVCamera* camera);
    private delegate nint       CameraCollisionDelegate(XIVCamera* a1, Vector3* a2, Vector3* a3, float a4, nint a5, float a6);
    private delegate nint       CameraUpdateDelegate(XIVCamera* camera);
    private delegate nint       CameraSceneUpdateDelegate(XIVSceneCamera* gsc);
    private delegate Matrix4x4* ProjectionMatrixDelegate(nint ptr, float fov, float aspect, float nearPlane, float farPlane, float a6, float a7);
    private delegate void       CameraMatrixLoadDelegate(XIVRenderCamera* camera, nint a1);
    
    
    [Signature("E8 ?? ?? ?? ?? F7 80 84 01 00 00 FB FF FF FF", DetourName = nameof(CameraManager_GetActiveCameraDetour))]
    private readonly Hook<CameraManager_GetActiveCameraDelegate>? CameraManager_GetActiveCameraHook = null;

    [Signature("E8 ?? ?? ?? ?? EB 03 48 8B C6 45 33 C0 48 89 07", DetourName = nameof(Camera_CtorDetour))]
    private readonly Hook<Camera_CtorDelegate>? Camera_CtorHook = null;

    
    [Signature("E8 ?? ?? ?? ?? 4C 8D 44 24 40 89 83 14 ?? ?? ??", DetourName = nameof(CameraCollisionDetour))]
    private readonly Hook<CameraCollisionDelegate>? CameraCollisionHook = null!;

    [Signature("40 55 53 57 48 8D 6C 24 A0 48 81 EC ?? ?? ?? ?? 48 8B 1D", DetourName = nameof(CameraUpdateDetour))]
    private readonly Hook<CameraUpdateDelegate>? CameraUpdateHook = null!;

    [Signature("48 ?? ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 F0 ?? ?? ?? ?? 48 8B ??", DetourName = nameof(CameraSceneUpdateDetour))]
    private readonly Hook<CameraSceneUpdateDelegate>? CameraSceneUpdateHook = null!;

    [Signature("E8 ?? ?? ?? ?? EB ?? F3 0F ?? ?? ?? ?? ?? ?? F3 0F ?? ?? ?? ?? E8 ?? ?? ?? ?? 0F ?? ?? ?? 48 ?? ?? ??", DetourName = nameof(ProjectionMatrixDetour))]
    private static Hook<ProjectionMatrixDelegate>? ProjectionHook = null!;
    
    
    private readonly CameraMatrixLoadDelegate CameraMatrixLoad;
    
    
    private MirrorCamera? OverrideCamera;

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
        : base(dalamudServices, mirrorServices)
    {
        nint cameraMatrixLoadAddr = DalamudServices.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B 93 90 02 ?? ?? 48 8D 4C 24 40");
        CameraMatrixLoad          = Marshal.GetDelegateForFunctionPointer<CameraMatrixLoadDelegate>(cameraMatrixLoadAddr);
    }

    public override void Init()
    {
        CameraManager_GetActiveCameraHook?.Enable();
        Camera_CtorHook?.Enable();

        EnvironmentManagerUpdateHook?.Enable();
        
        CameraCollisionHook?.Enable();
        CameraUpdateHook?.Enable();
        CameraSceneUpdateHook?.Enable();
        ProjectionHook?.Enable();
    }
    
    public override void Dispose()
    {
        EnvironmentManagerUpdateHook?.Dispose();

        CameraManager_GetActiveCameraHook?.Dispose();
        Camera_CtorHook?.Dispose();
        
        CameraCollisionHook?.Dispose();
        CameraUpdateHook?.Dispose();
        CameraSceneUpdateHook?.Dispose();
        ProjectionHook?.Dispose();
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

        XIVCamera* activeCamera = Control.Instance()->CameraManager.GetActiveCamera();

        if (activeCamera == null)
        {
            return;
        }

        XIVRenderCamera* renderCamera = activeCamera->SceneCamera.RenderCamera;

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

    public GameAllocation<XIVCamera> SpawnCamera(XIVCamera* clone = null)
    {
        GameAllocation<XIVCamera> newCamera = new GameAllocation<XIVCamera>();

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

    private nint CameraCollisionDetour(XIVCamera* a1, Vector3* a2, Vector3* a3, float a4, nint a5, float a6)
    {
        MirrorServices.MirrorLog.Log("CameraCollisionDetour");
        
        return CameraCollisionHook!.OriginalDisposeSafe(a1, a2, a3, a4, a5, a6);
    }
    
    private nint CameraUpdateDetour(XIVCamera* camera)
    {
        MirrorServices.MirrorLog.Log("CameraUpdateDetour");
        
        nint returner = CameraUpdateHook!.Original(camera);
        
        
        if (OverrideCamera != null)
        {
            camera->SceneCamera.Object.Position = OverrideCamera.Camera->SceneCamera.Object.Position;
            camera->SceneCamera.LookAtVector    = OverrideCamera.Camera->SceneCamera.LookAtVector;
            camera->SceneCamera.Object.Rotation = OverrideCamera.Camera->SceneCamera.Object.Rotation;
            camera->SceneCamera.Rotation        = OverrideCamera.Camera->SceneCamera.Rotation;
            camera->SceneCamera.ViewMatrix      = OverrideCamera.Camera->SceneCamera.ViewMatrix;
            camera->SceneCamera.RenderCamera->ProjectionMatrix = OverrideCamera.Camera->SceneCamera.RenderCamera->ProjectionMatrix;
            camera->SceneCamera.RenderCamera->ViewMatrix = OverrideCamera.Camera->SceneCamera.RenderCamera->ViewMatrix;
            camera->SceneCamera.RenderCamera->ProjectionMatrix2 = OverrideCamera.Camera->SceneCamera.RenderCamera->ProjectionMatrix2;
        }
        
        return returner;
    }
    
    private nint CameraSceneUpdateDetour(XIVSceneCamera* gsc)
    {
        MirrorServices.MirrorLog.Log("CameraSceneUpdateDetour");
        
        return CameraSceneUpdateHook!.Original(gsc);        
    }
    
    private Matrix4x4* ProjectionMatrixDetour(nint ptr, float fov, float aspect, float nearPlane, float farPlane, float a6, float a7)
    {
        
        MirrorServices.MirrorLog.Log("ProjectionMatrixDetour");
        
        return ProjectionHook!.OriginalDisposeSafe(ptr, fov, aspect, nearPlane, farPlane, a6, a7);
    }
    
    private XIVCamera* CameraManager_GetActiveCameraDetour(CameraManager* cameraManager)
    {
        MirrorServices.MirrorLog.Log("CameraManager_GetActiveCameraDetour");
        
        return CameraManager_GetActiveCameraHook!.Original(cameraManager);
    }

    private XIVCamera* Camera_CtorDetour(XIVCamera* camera)
    {
        MirrorServices.MirrorLog.Log("Camera Constructor Triggered.");

        return Camera_CtorHook!.Original(camera);
    }
}
