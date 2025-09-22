using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using MirrorsEdge.XIVMirrors.Cameras.CameraTypes;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX;
using System;
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

    private delegate nint GetEngineCoreSingletonDelegate();

    [Signature("E8 ?? ?? ?? ?? 48 8D 4C 24 ?? 48 89 4C 24 ?? 4C 8D 4D ?? 4C 8D 44 24 ??", DetourName = nameof(EngineCoreSingletonDetour))]
    private readonly Hook<GetEngineCoreSingletonDelegate>? GetEngineCoreSingletonHook = null;

    private nint EngineCoreSingleton;

    private Matrix ViewProjMatrix   = Matrix.Identity;
    private Matrix ViewMatrix       = Matrix.Identity;
    private Matrix ProjectionMatrix = Matrix.Identity;

    public CameraHooks(DalamudServices dalamudServices, MirrorServices mirrorServices, RendererHook rendererHook) : base(dalamudServices, mirrorServices) 
    {
        RendererHook   = rendererHook;

        RendererHook.RegisterRenderPassListener(OnRenderPass);
    }

    public override void Init()
    {
        CameraManager_GetActiveCameraHook?.Enable();
        Camera_CtorHook?.Enable();

        try
        {
            EngineCoreSingleton = EngineCoreSingletonDetour();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    private nint EngineCoreSingletonDetour()
    {
        return GetEngineCoreSingletonHook!.Original();
    }

    public unsafe Matrix ReadMatrix(IntPtr address)
    {
        float* p = (float*)address;

        Matrix matrix = new Matrix();

        for (int index = 0; index < 16; index++)
        {
            matrix[index] = *p++;
        }

        return matrix;
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (EngineCoreSingleton == nint.Zero)
        {
            return;
        }

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

        try
        {
            ViewProjMatrix      = ReadMatrix(EngineCoreSingleton + 0x1B4);
            ProjectionMatrix    = ReadMatrix(EngineCoreSingleton + 0x174);
            ViewMatrix          = ReadMatrix(EngineCoreSingleton + 0x134);

            MirrorServices.MirrorLog.LogVerbose("Standard Z: " + renderCamera->FiniteFarPlane);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }
    
    public Matrix GetMatrix()
    {
        Matrix worldMatrix = Matrix.Identity;

        Matrix viewProjMatrix = ViewProjMatrix;
        viewProjMatrix.Transpose();

        Matrix wvp          = worldMatrix * viewProjMatrix;

        return wvp;
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

        GetEngineCoreSingletonHook?.Dispose();

        CameraManager_GetActiveCameraHook?.Dispose();
        Camera_CtorHook?.Dispose();
    }
}
