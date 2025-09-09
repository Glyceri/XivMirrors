using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.Structs;
using MirrorsEdge.MirrorsEdge.Services;
using System;
using SceneCameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    public  delegate bool RenderPassDelegate(RenderPass pass);
    private delegate void DXGIPresentDelegate(long a, long b);
    private delegate void RenderThreadSetRenderTargetDelegate(Device* deviceInstance, SetRenderTargetCommand* command);
    private delegate void SetMatricesDelegate(FFXIVClientStructs.FFXIV.Client.Game.Camera* camera, IntPtr ptr);

    [Signature("E8 ?? ?? ?? ?? C6 43 79 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 5F 18", DetourName = nameof(RenderThreadSetRenderTargetDetour))]
    private Hook<RenderThreadSetRenderTargetDelegate>? RenderThreadSetRenderTargetHook = null;

    [Signature("E8 ?? ?? ?? ?? 0F 10 43 ?? C6 83", DetourName = nameof(SetMatricesDetour))]
    private Hook<SetMatricesDelegate>? SetMatricesHook = null;

    private RenderPassDelegate? _renderPass = null!;

    private readonly CameraHooks CameraHooks;

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHooks cameraHooks) : base(dalamudServices, mirrorServices)
    {
        CameraHooks = cameraHooks;
    }

    public override void Init()
    {
        DXGIPresentHook?.Enable();
        //RenderThreadSetRenderTargetHook?.Enable();
        //SetMatricesHook?.Enable();
    }

    private void DXGIPresentDetour(long a, long b)
    {
        try
        {
            if (_renderPass?.Invoke(RenderPass.Mirror) ?? false)
            {
                DXGIPresentHook!.Original(a, b);
            }

            CameraHooks.SetOverride(null);

            if (_renderPass?.Invoke(RenderPass.Main) ?? true)
            {
                DXGIPresentHook!.Original(a, b);
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }
    }

    private void RenderThreadSetRenderTargetDetour(Device* deviceInstance, SetRenderTargetCommand* command)
    {
        int renderTargets = command->NumberOfRenderTargets;

        //MirrorServices.MirrorLog.Log("Render Targets: " + renderTargets);

        RenderThreadSetRenderTargetHook!.Original(deviceInstance, command);
    }

    private void SetMatricesDetour(FFXIVClientStructs.FFXIV.Client.Game.Camera* camera, IntPtr ptr)
    {
        SetMatricesHook!.Original(camera, ptr);

        SceneCameraManager* cameraManager = SceneCameraManager.Instance();

        if (cameraManager == null)
        {
            return;
        }

        SceneCamera* sceneCamera = cameraManager->CurrentCamera;

        if (sceneCamera == null)
        {
            return;
        }

        RenderCamera* renderCamera = sceneCamera->RenderCamera;

        if (renderCamera == null)
        {
            return;
        }

        if (camera == renderCamera)
        {
            // Same Camera
        }
    }

    public void SetRenderPassListener(RenderPassDelegate? renderDelegate)
    {
        _renderPass = renderDelegate;
    }

    public override void OnDispose()
    {
        RenderThreadSetRenderTargetHook?.Dispose();
        SetMatricesHook?.Dispose();
        DXGIPresentHook?.Dispose();
    }
}
