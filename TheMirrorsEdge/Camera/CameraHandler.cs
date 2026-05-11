using System;
using System.Collections.Generic;
using TheMirrorsEdge.Camera.CameraTypes;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using TheMirrorsEdge.Hooking.Elements;
using XIVCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;

namespace TheMirrorsEdge.Camera;

public unsafe class CameraHandler : IDisposable
{
    private readonly List<BaseCamera> _cameras = new List<BaseCamera>();

    public NativeCamera? GameCamera { get; private set; }

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly CameraHook         CameraHook;

    private BaseCamera? _currentActiveCamera;

    public CameraHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHook cameraHooks)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        CameraHook      = cameraHooks;

        PrepareCameraList();

        SetActiveCamera(GameCamera);
    }

    public BaseCamera? ActiveCamera 
        => _currentActiveCamera;
    
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

        XIVCamera* camera = cameraManager->Camera;
        
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
            CameraHook.SetOverride(mirrorCamera);
        }
        else
        {
            CameraHook.SetOverride(null);
        }

        _currentActiveCamera = camera;
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

        XIVCamera* camera = cameraManager->Camera;

        if (camera == null)
        {
            return null;
        }

        GameAllocation<XIVCamera> spawnedCamera   = CameraHook.SpawnCamera(camera);
        MirrorCamera              newMirrorCamera = new MirrorCamera(ref spawnedCamera);

        RegisterNewCamera(newMirrorCamera);

        return newMirrorCamera;
    }

    public void DestroyCamera(BaseCamera camera)
    {
        if (_currentActiveCamera == camera)
        {
            ResetCamera();
        }

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
        DisposeCameras();
    }
}