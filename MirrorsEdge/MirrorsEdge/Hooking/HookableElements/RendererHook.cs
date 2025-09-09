using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.Structs;
using MirrorsEdge.MirrorsEdge.Services;
using Serilog.Core;
using System;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;
using SceneCameraManager = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CameraManager;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    private bool _disposed = false;

    private delegate nint SomeRenderDelegate(nint a1, nint a2, nint a3, nint a4);
    public  delegate bool RenderPassDelegate(RenderPass pass);
    private delegate void DXGIPresentDelegate(IntPtr ptr);
    private delegate void RenderThreadSetRenderTargetDelegate(Device* deviceInstance, SetRenderTargetCommand* command);
    private delegate void SetMatricesDelegate(FFXIVClientStructs.FFXIV.Client.Game.Camera* camera, IntPtr ptr);
    private delegate IntPtr RenderDelegate(Manager* manager);

    private delegate void ImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3);

    [Signature("E8 ?? ?? ?? ?? 48 8B 0D ?? ?? ?? ?? 48 8B 81 28 02 00 00", DetourName = nameof(RenderDetour))]
    private readonly Hook<RenderDelegate>? RenderHook = null;

    [Signature("40 53 55 57 41 55 48 83 EC 68", DetourName = nameof(SomeRenderDetour))]
    private readonly Hook<SomeRenderDelegate>? SomeRenderHook = null;

    [Signature("E8 ?? ?? ?? ?? 48 8B 4B 30 FF 15 ?? ?? ?? ??", DetourName = nameof(OnImmediateContextProcessCommands))]
    private readonly Hook<ImmediateContextProcessCommands>? ImmediateContextProcessCommandsHook = null;

    [Signature("E8 ?? ?? ?? ?? C6 46 79 00 48 8B 8E 88 0A 0E 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 5F 18", DetourName = nameof(RenderThreadSetRenderTargetDetour))]
    private Hook<RenderThreadSetRenderTargetDelegate>? RenderThreadSetRenderTargetHook = null;

    [Signature("E8 ?? ?? ?? ?? 0F 10 43 ?? C6 83", DetourName = nameof(SetMatricesDetour))]
    private Hook<SetMatricesDelegate>? SetMatricesHook = null;

    private RenderPassDelegate? _renderPass = null!;
    private RenderPassDelegate? _postRender = null!;

    private readonly CameraHooks CameraHooks;

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHooks cameraHooks) : base(dalamudServices, mirrorServices)
    {
        CameraHooks = cameraHooks;
    }

    public override void Init()
    {
        //RenderHook?.Enable();
        //SomeRenderHook?.Enable();
        //ImmediateContextProcessCommandsHook?.Enable();
        DXGIPresentHook?.Enable();
        //RenderThreadSetRenderTargetHook?.Enable();
        //SetMatricesHook?.Enable();
    }

    private IntPtr RenderDetour(Manager* manager)
    {
        MirrorServices.MirrorLog.Log("On Render");



        IntPtr yaya =  RenderHook!.Original(manager);

        if (_renderPass?.Invoke(RenderPass.Main) ?? false)
        {

        }

        return yaya;
    }

    private nint SomeRenderDetour(nint a1, nint a2, nint a3, nint a4)
    {
        if (_renderPass?.Invoke(RenderPass.Mirror) ?? false)
        {

        }

        nint outcome = SomeRenderHook!.Original(a1, a2, a3, a4);

       

            _postRender?.Invoke(RenderPass.Mirror);

        if (_renderPass?.Invoke(RenderPass.Main) ?? false)
        {

        }

        outcome = SomeRenderHook!.Original(a1, a2, a3, a4);

        _postRender?.Invoke(RenderPass.Main);


        return outcome;
    }

    private void OnImmediateContextProcessCommands(ImmediateContext* commands, RenderCommandBufferGroup* bufferGroup, uint a3)
    {
        try
        {
            currentPass = RenderPass.Mirror;

            if (_renderPass?.Invoke(RenderPass.Mirror) ?? false)
            {
                //ImmediateContextProcessCommandsHook!.OriginalDisposeSafe(commands, bufferGroup, a3);
            }

            _postRender?.Invoke(RenderPass.Mirror);

            currentPass = RenderPass.Main;

            if (_renderPass?.Invoke(RenderPass.Main) ?? true)
            {
                ImmediateContextProcessCommandsHook!.OriginalDisposeSafe(commands, bufferGroup, a3);
            }

            _postRender?.Invoke(RenderPass.Main);
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }     
    }

    private RenderPass currentPass = RenderPass.Main;

    private void DXGIPresentDetour(IntPtr ptr)
    {
        try
        {
            currentPass = RenderPass.Mirror;
            
            if (_renderPass?.Invoke(RenderPass.Mirror) ?? false)
            {
                DXGIPresentHook!.Original(ptr);
            }

            _postRender?.Invoke(currentPass);

            currentPass = RenderPass.Main;

            if (_renderPass?.Invoke(RenderPass.Main) ?? true)
            {
                DXGIPresentHook!.Original(ptr);
            }

            _postRender?.Invoke(currentPass);
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

    public void SetRenderPassListener(RenderPassDelegate? renderDelegate, RenderPassDelegate? postRender)
    {
        _renderPass = renderDelegate;
        _postRender = postRender;
    }

    public override void OnDispose()
    {
        _disposed = true;

        RenderHook?.Dispose();

        SomeRenderHook?.Dispose();

        ImmediateContextProcessCommandsHook?.Dispose();

        DXGIPresentHook?.Disable();

        RenderThreadSetRenderTargetHook?.Dispose();
        SetMatricesHook?.Dispose();
        DXGIPresentHook?.Dispose();
    }
}
