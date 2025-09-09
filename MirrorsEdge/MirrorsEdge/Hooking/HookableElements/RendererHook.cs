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
using FFXIVClientStructs.FFXIV.Client.Game;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    private bool _disposed = false;

    public  delegate bool RenderPassDelegate(RenderPass pass);
    private delegate void DXGIPresentDelegate(IntPtr ptr);
    private delegate void RenderThreadSetRenderTargetDelegate(Device* deviceInstance, SetRenderTargetCommand* command);
    private delegate void SetMatricesDelegate(FFXIVClientStructs.FFXIV.Client.Game.Camera* camera, IntPtr ptr);

    private delegate void ImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3);

    [Signature("E8 ?? ?? ?? ?? 48 8B 4B 30 FF 15 ?? ?? ?? ??", DetourName = nameof(OnImmediateContextProcessCommands))]
    private readonly Hook<ImmediateContextProcessCommands>? ImmediateContextProcessCommandsHook = null;

    [Signature("E8 ?? ?? ?? ?? C6 46 79 00 48 8B 8E 88 0A 0E 00", DetourName = nameof(DXGIPresentDetour))]
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
        //ImmediateContextProcessCommandsHook?.Enable();
        DXGIPresentHook?.Enable();
        //RenderThreadSetRenderTargetHook?.Enable();
        //SetMatricesHook?.Enable();
    }

    private void OnImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3)
    {
        ImmediateContextProcessCommandsHook!.OriginalDisposeSafe(commands, bufferGroup, a3);

        try
        {
            if (_renderPass?.Invoke(RenderPass.Main) ?? true)
            {
                ImmediateContextProcessCommandsHook!.OriginalDisposeSafe(commands, bufferGroup, a3);
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }        
    }

    private void DXGIPresentDetour(IntPtr ptr)
    {
        try
        {
            if (_renderPass?.Invoke(RenderPass.Mirror) ?? false)
            {
                DXGIPresentHook!.OriginalDisposeSafe(ptr);
            }

            CameraHooks.SetOverride(null);
           
            if (_renderPass?.Invoke(RenderPass.Main) ?? true)
            {
                DXGIPresentHook!.OriginalDisposeSafe(ptr);
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
        _disposed = true;

        ImmediateContextProcessCommandsHook?.Dispose();

        DXGIPresentHook?.Disable();

        RenderThreadSetRenderTargetHook?.Dispose();
        SetMatricesHook?.Dispose();
        DXGIPresentHook?.Dispose();
    }
}
