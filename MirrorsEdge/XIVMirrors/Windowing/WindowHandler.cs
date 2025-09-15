using Dalamud.Interface.Windowing;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Cameras;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using MirrorsEdge.XIVMirrors.Windowing.Interfaces;
using MirrorsEdge.XIVMirrors.Windowing.Windows;
using System;
using System.Linq;

namespace MirrorsEdge.XIVMirrors.Windowing;

internal class WindowHandler : IDisposable
{
    private static int _internalCounter = 0;
    public static int InternalCounter { get => _internalCounter++; }

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly CameraHandler      CameraHandler;
    private readonly RendererHook       RendererHook;
    private readonly ScreenHook         ScreenHook;
    private readonly ShaderHandler      ShaderHandler;
    private readonly DirectXData        DirectXData;
    private readonly BackBufferHook     BackBufferHook;

    private readonly WindowSystem       WindowSystem;

    public WindowHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderFactory, DirectXData directXData, BackBufferHook backBufferHook)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        CameraHandler   = cameraHandler;
        RendererHook    = rendererHook;
        ShaderHandler   = shaderFactory;
        ScreenHook      = screenHook;
        DirectXData     = directXData;
        BackBufferHook = backBufferHook;    

        WindowSystem = new WindowSystem("Mirrors");

        DalamudServices.DalamudPlugin.UiBuilder.Draw += Draw;

        _Register();
    }

    private void _Register()
    {
        DalamudServices.Framework.RunOnFrameworkThread(() => AddWindow(new DebugWindow(this, DalamudServices, MirrorServices, CameraHandler, RendererHook, ScreenHook, ShaderHandler, DirectXData, BackBufferHook)));
    }

    private void AddWindow(MirrorWindow window)
    {
        WindowSystem.AddWindow(window);
    }

    public void Open<T>() where T : IMirrorWindow
    {
        foreach (IMirrorWindow window in WindowSystem.Windows)
        {
            if (window is not T tWindow) continue;

            tWindow.Open();
        }
    }

    public void Close<T>() where T : IMirrorWindow
    {
        foreach (IMirrorWindow window in WindowSystem.Windows)
        {
            if (window is not T tWindow) continue;

            tWindow.Close();
        }
    }

    public void Toggle<T>() where T : IMirrorWindow
    {
        foreach (IMirrorWindow window in WindowSystem.Windows)
        {
            if (window is not T tWindow) continue;

            tWindow.Toggle();
        }
    }

    public T? GetWindow<T>() where T : IMirrorWindow
    {
        return WindowSystem.Windows.OfType<T>().FirstOrDefault();
    }

    private void Draw()
    {
        ScreenHook.OnImGuiDraw();

        _internalCounter = 0;

        WindowSystem.Draw();
    }

    public void Dispose()
    {
        DalamudServices.DalamudPlugin.UiBuilder.Draw -= Draw;

        foreach (IMirrorWindow window in WindowSystem.Windows)
        {
            window.Dispose();
        }
    }
}
