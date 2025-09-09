using Dalamud.Interface.Windowing;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Shaders;
using MirrorsEdge.MirrorsEdge.Windowing.Interfaces;
using MirrorsEdge.MirrorsEdge.Windowing.Windows;
using System;
using System.Linq;

namespace MirrorsEdge.MirrorsEdge.Windowing;

internal class WindowHandler : IDisposable
{
    private static int _internalCounter = 0;
    public static int InternalCounter { get => _internalCounter++; }

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly CameraHandler      CameraHandler;
    private readonly TextureHooker      TextureHooker;
    private readonly RendererHook       RendererHook;
    private readonly ShaderFactory      ShaderFactory;

    private readonly WindowSystem       WindowSystem;

    public WindowHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook, ShaderFactory shaderFactory)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        CameraHandler   = cameraHandler;
        TextureHooker   = textureHooker;
        RendererHook    = rendererHook;
        ShaderFactory   = shaderFactory;

        WindowSystem = new WindowSystem("Mirrors");

        DalamudServices.DalamudPlugin.UiBuilder.Draw += Draw;

        _Register();
    }

    private void _Register()
    {
        AddWindow(new DebugWindow(this, DalamudServices, MirrorServices, CameraHandler, TextureHooker, RendererHook, ShaderFactory));
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
