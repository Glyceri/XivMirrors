using Dalamud.Bindings.ImGui;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Cameras;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using System;
using MirrorsEdge.XIVMirrors.Resources;
using System.Numerics;

namespace MirrorsEdge.XIVMirrors.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    private bool _disposed = false;

    protected override Vector2 MinSize      { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize      { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize  { get; } = new Vector2(800, 400);

    private readonly CameraHandler              CameraHandler;
    private readonly RendererHook               RendererHook;
    private readonly ScreenHook                 ScreenHook;
    private readonly ShaderHandler              ShaderHandler;
    private readonly DirectXData                DirectXData;
    private readonly BackBufferHook             BackBufferHook;
    private readonly CubeRenderHook             CubeRenderHook;
    private readonly TransparentBackBufferHook  TransparentBackBufferHook;


    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderFactory, DirectXData directXData, BackBufferHook backBufferHook, TransparentBackBufferHook tBackBufferHook, CubeRenderHook cubeRenderHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler               = cameraHandler;
        RendererHook                = rendererHook;
        ScreenHook                  = screenHook;
        ShaderHandler               = shaderFactory;
        DirectXData                 = directXData;
        BackBufferHook              = backBufferHook;
        CubeRenderHook              = cubeRenderHook;
        TransparentBackBufferHook   = tBackBufferHook;

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreensizeChanged);

        RendererHook.RegisterRenderPassListener(OnRenderPass);

        Open();
    }
    
    private void OnRenderPass(RenderPass renderPass)
    {
        
    }

    private void OnScreensizeChanged(uint newWidth, uint newHeight)
    {
        
    }

    private void DrawMappedTexture(RenderTarget? mappedTexture, Vector2 size)
    {
        if (mappedTexture == null)
        {
            return;
        }

        ImGui.Image(mappedTexture.Handle, size);
    }

    private void DrawBackBuffer()
    {
        Vector2 size = ImGui.GetContentRegionAvail();

        if (CubeRenderHook.FinalRenderTarget != null)
        {
            ImGui.Image(CubeRenderHook.FinalRenderTarget.Handle, size);
        }

        if (CubeRenderHook.OutputView != null)
        {
            ImGui.Image(new ImTextureID(CubeRenderHook.OutputView.NativePointer), size * 0.5f);
        }

        ImGui.SameLine();

        if (CubeRenderHook.DepthView != null)
        {
            ImGui.Image(new ImTextureID(CubeRenderHook.DepthView.NativePointer), size * 0.5f);
        }

        ImGui.NewLine();



        if (BackBufferHook.BackBufferNoUIBase != null)
        {
            ImGui.Image(BackBufferHook.BackBufferNoUIBase.Handle, new Vector2(500, 500));
        }

        ImGui.SameLine();

        if (BackBufferHook.BackBufferWithUIBase != null)
        {
            ImGui.Image(BackBufferHook.BackBufferWithUIBase.Handle, new Vector2(500, 500));
        }

        ImGui.NewLine();




        if (BackBufferHook.NonTransparentDepthBufferBase != null)
        {
            ImGui.Image(BackBufferHook.NonTransparentDepthBufferBase.Handle, new Vector2(500, 500));
        }

        ImGui.SameLine();

        if (BackBufferHook.TransparentDepthBufferBase != null)
        {
            ImGui.Image(BackBufferHook.TransparentDepthBufferBase.Handle, new Vector2(500, 500));
        }

        ImGui.NewLine();


        if (BackBufferHook.SecondDalamudBackBufferBase != null)
        {
            ImGui.Image(BackBufferHook.SecondDalamudBackBufferBase.Handle, new Vector2(500, 500));
        }

        ImGui.NewLine();




        if (BackBufferHook.DalamudBackBuffer != null)
        {
            ImGui.Image(BackBufferHook.DalamudBackBuffer.Handle, new System.Numerics.Vector2(500, 500));
        }

        ImGui.SameLine();

        if (BackBufferHook.SecondDalamudBackBuffer != null)
        {
            ImGui.Image(BackBufferHook.SecondDalamudBackBuffer.Handle, new System.Numerics.Vector2(500, 500));
        }

        //if (BackBufferHook.ThatOneSecretTexture != null)
        {
            //ImGui.Image(BackBufferHook.ThatOneSecretTexture.Handle, new System.Numerics.Vector2(500, 500));
        }

        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();

        Vector2 defaultSize = new Vector2(500, 500);

        DrawMappedTexture(TransparentBackBufferHook.FinalRenderTarget, new Vector2(1920, 1080));

        DrawMappedTexture(TransparentBackBufferHook.RtTransparentDiffuseMap, defaultSize);
        ImGui.SameLine();
        DrawMappedTexture(TransparentBackBufferHook.RtTransparentStrengthMap, defaultSize);

        DrawMappedTexture(TransparentBackBufferHook.RtTransparentDiffuseLightMap, defaultSize);
        ImGui.SameLine();
        DrawMappedTexture(TransparentBackBufferHook.RtTransparentRealTimeLightMap, defaultSize);

        DrawMappedTexture(TransparentBackBufferHook.RtTransparentSpecularLightMap, defaultSize);

        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();
        ImGui.NewLine();

        DrawMappedTexture(BackBufferHook.BackBufferNoUI, defaultSize);
        ImGui.SameLine();
        DrawMappedTexture(BackBufferHook.BackBufferWithUI, defaultSize);
        DrawMappedTexture(BackBufferHook.DepthBufferNoTransparency, defaultSize);
        ImGui.SameLine();
        DrawMappedTexture(BackBufferHook.DepthBufferWithTransparency, defaultSize);
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

                //DalamudServices.Framework.RunOnTick(() => cameraHasChanged = true, delayTicks: 2);
            }

            ImGui.SameLine();

            if (ImGui.Button($"X##{WindowHandler.InternalCounter}"))
            {
                //_ = DalamudServices.Framework.RunOnFrameworkThread(() => CameraHandler.DestroyCamera(camera));
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
