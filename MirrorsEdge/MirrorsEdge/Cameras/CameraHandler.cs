using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Memory;
using MirrorsEdge.MirrorsEdge.Services;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Cameras;

internal unsafe class CameraHandler : IDisposable
{
    private readonly List<BaseCamera> _cameras = new List<BaseCamera>();

    public NativeCamera? GameCamera { get; private set; }

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly CameraHooks        CameraHooks;

    public CameraHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHooks cameraHooks)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        CameraHooks     = cameraHooks;

        PrepareCameraList();
    }

    public BaseCamera[] Cameras =>
        _cameras.ToArray();

    public void RegisterNewCamera(BaseCamera camera)
    {
        _ = _cameras.Remove(camera);

        _cameras.Add(camera);
    }

    public void PrepareCameraList()
    {
        MirrorServices.MirrorLog.Log("Preparing Camera List");

        DisposeCameras();

        _cameras.Clear();

        CameraManager* cameraManager = CameraManager.Instance();

        if (cameraManager == null)
        {
            return;
        }

        Camera* camera = cameraManager->Camera;
        
        if (camera == null)
        {
            return;
        }

        NativeCamera nativeCamera = new NativeCamera(camera);

        RegisterNewCamera(nativeCamera);

        GameCamera = nativeCamera;
    }

    public void SetActiveCamera(BaseCamera? camera)
    {
        MirrorServices.MirrorLog.Log("Overwriting Active Camera");

        FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager* cameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager.Instance();

        if (cameraManager == null)
        {
            return;
        }

        if (camera == null)
        {
            ResetCamera();

            return;
        }

        cameraManager->Cameras[0] = &camera.Camera->SceneCamera;

        if (camera is MirrorCamera mirrorCamera)
        {
            CameraHooks.SetOverride(mirrorCamera);
        }
        else
        {
            CameraHooks.SetOverride(null);
        }
    }

    public void ResetCamera()
    {
        if (GameCamera == null)
        {
            return;
        }

        SetActiveCamera(GameCamera);
    }

    public MirrorCamera? CreateCamera()
    {
        CameraManager* cameraManager = CameraManager.Instance();

        if (cameraManager == null)
        {
            return null;
        }

        Camera* camera = cameraManager->Camera;

        if (camera == null)
        {
            return null;
        }

        GameAllocation<Camera> spawnedCamera = CameraHooks.SpawnCamera(camera);

        MirrorCamera newMirrorCamera = new MirrorCamera(ref spawnedCamera);

        RegisterNewCamera(newMirrorCamera);

        return newMirrorCamera;
    }

    public void DestroyCamera(BaseCamera camera)
    {
        if (camera is MirrorCamera)
        {
            camera.Dispose();
        }

        _ = _cameras.Remove(camera);
    }

    private void DisposeCameras()
    {
        for (int i = _cameras.Count - 1; i >= 0; i--)
        {
            BaseCamera camera = _cameras[i];

            DestroyCamera(camera);
        }
    }

    public void Dispose()
    {
        
    }
}
