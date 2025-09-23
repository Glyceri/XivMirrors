using MirrorsEdge.XIVMirrors.Cameras;
using MirrorsEdge.XIVMirrors.Commands;
using MirrorsEdge.XIVMirrors.Hooking;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.ResourceHandling;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using MirrorsEdge.XIVMirrors.Windowing;
using System;

namespace MirrorsEdge;

internal class MirrorsEdgeStarter : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;

    private readonly DirectXData        DirectXData;

    private readonly HookManager        HookManager;
    private readonly ResourceHandler    ResourceHandler;
    private readonly CameraHandler      CameraHandler;
    private readonly WindowHandler      WindowHandler;

    private readonly ResourceLoader     ResourceLoader;
    private readonly ShaderHandler      ShaderHandler;

    private readonly CommandHandler     CommandHandler;

    public MirrorsEdgeStarter(DalamudServices dalamudServices)
    {
        DalamudServices     = dalamudServices;

        MirrorServices      = new MirrorServices(DalamudServices);

        DirectXData         = new DirectXData(MirrorServices);

        ResourceLoader      = new ResourceLoader(DirectXData);

        ShaderHandler       = new ShaderHandler(MirrorServices, ResourceLoader, DirectXData);

        ResourceHandler     = new ResourceHandler(DalamudServices, MirrorServices);

        HookManager         = new HookManager(DalamudServices, MirrorServices, DirectXData, ShaderHandler, ResourceHandler);

        CameraHandler       = new CameraHandler(DalamudServices, MirrorServices, HookManager.CameraHooks);

        WindowHandler       = new WindowHandler(DalamudServices, MirrorServices, CameraHandler, HookManager.RendererHook, HookManager.ScreenHook, ShaderHandler, DirectXData, HookManager.BackBufferHook, HookManager.CubeRenderHook, HookManager.TransparentBackBufferHook);

        CommandHandler      = new CommandHandler(DalamudServices, MirrorServices, WindowHandler);
    }

    public void Dispose()
    {
        CommandHandler?.Dispose();

        ResourceHandler?.Dispose();

        HookManager?.Dispose();
        WindowHandler?.Dispose();
        CameraHandler?.Dispose();

        DirectXData?.Dispose();
    }
}
