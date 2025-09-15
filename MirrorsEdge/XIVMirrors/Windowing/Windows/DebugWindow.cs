using Dalamud.Bindings.ImGui;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Cameras;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using System;
using MirrorsEdge.XIVMirrors.Resources;

namespace MirrorsEdge.XIVMirrors.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    private bool _disposed = false;

    protected override System.Numerics.Vector2 MinSize      { get; } = new System.Numerics.Vector2(350, 136);
    protected override System.Numerics.Vector2 MaxSize      { get; } = new System.Numerics.Vector2(2000, 2000);
    protected override System.Numerics.Vector2 DefaultSize  { get; } = new System.Numerics.Vector2(800, 400);

    private readonly CameraHandler  CameraHandler;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;
    private readonly DirectXData    DirectXData;
    private readonly BackBufferHook BackBufferHook;

    private bool cameraHasChanged = false;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderFactory, DirectXData directXData, BackBufferHook backBufferHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler   = cameraHandler;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderFactory;
        DirectXData     = directXData;
        BackBufferHook  = backBufferHook;

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreensizeChanged);

        RendererHook.RegisterRenderPassListener(OnRenderPass);

        Open();
    }
    
    private void OnRenderPass(RenderPass renderPass)
    {
        
    }

    private void OnScreensizeChanged(int newWidth, int newHeight)
    {
        
    }

    private void DrawMappedTexture(RenderTarget? mappedTexture)
    {
        if (mappedTexture == null)
        {
            return;
        }

        ImGui.Image(mappedTexture.Handle, new System.Numerics.Vector2(500, 500));
    }

    private void DrawBackBuffer()
    {
        DrawMappedTexture(BackBufferHook.BackBufferNoUI);
        ImGui.SameLine();
        DrawMappedTexture(BackBufferHook.BackBufferWithUI);
        DrawMappedTexture(BackBufferHook.DepthBufferNoTransparency);
        ImGui.SameLine();
        DrawMappedTexture(BackBufferHook.DepthBufferWithTransparency);
    }

    protected override void OnDraw()
    {
        if (_disposed)
        {
            return;
        }

        DrawBackBuffer();

        int camCounter = 0;

        if (ImGui.Button("Flood Camera List"))
        {
            CameraHandler.PrepareCameraList();
        }

        foreach (BaseCamera camera in CameraHandler.Cameras)
        {
            if (ImGui.Button($"[{camCounter}]: {camera.GetType().Name}"))
            {
                CameraHandler.SetActiveCamera(camera);

                DalamudServices.Framework.RunOnTick(() => cameraHasChanged = true, delayTicks: 2);
            }

            ImGui.SameLine();

            if (ImGui.Button($"X##{WindowHandler.InternalCounter}"))
            {
                _ = DalamudServices.Framework.RunOnFrameworkThread(() => CameraHandler.DestroyCamera(camera));
            }

            camCounter++;
        }

        if (ImGui.Button("Spawn Camera"))
        {
            try
            {
                _ = CameraHandler.CreateCamera();
            }
            catch(Exception e)
            {
                MirrorServices.MirrorLog.LogException(e);
            }
        }
    }

    protected override void OnDispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        RendererHook.DeregisterRenderPassListener(OnRenderPass);

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreensizeChanged);
    }
}
