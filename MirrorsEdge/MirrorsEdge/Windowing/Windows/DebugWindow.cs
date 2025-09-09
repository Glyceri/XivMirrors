using Dalamud.Bindings.ImGui;
using MirrorsEdge.mirrorsedge.Hooking.HookableElements;
using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

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

    private System.Numerics.Vector2 _screenSize = System.Numerics.Vector2.Zero;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderFactory, DirectXData directXData, BackBufferHook backBufferHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        RendererHook  = rendererHook;
        ScreenHook    = screenHook;
        ShaderHandler = shaderFactory;
        DirectXData   = directXData;
        BackBufferHook = backBufferHook;

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreensizeChanged);
        ScreenHook.SetupSize(ref _screenSize);

        Open();
    }

    private void OnScreensizeChanged(System.Numerics.Vector2 screenSize)
    {
        _screenSize = screenSize;
    }

    private void DrawBackBuffer()
    {
        try
        {
            System.Numerics.Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
            Size2 size = new Size2((int)contentRegionAvail.X, (int)contentRegionAvail.Y);

            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            ImGui.Image(BackBufferHook.BackBuffer?.ImGUIHandle ?? ImTextureID.Null, new(size.Width, size.Height));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnDraw()
    {
        if (_disposed)
        {
            return;
        }

        //GetBackBuffer();

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

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreensizeChanged);
    }
}
