using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Lumina.Data.Structs;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Cameras;
using MirrorsEdge.XIVMirrors.Hooking;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using MirrorsEdge.XIVMirrors.Windowing;
using System.Reflection;
using MirrorsEdge.XIVMirrors.ResourceHandling;

namespace MirrorsEdge;

public sealed class MirrorsEdgePlugin : IDalamudPlugin
{
    public readonly string Version;

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;

    private readonly DirectXData        DirectXData;

    private readonly HookManager        HookManager;
    private readonly ResourceHandler    ResourceHandler;
    private readonly CameraHandler      CameraHandler;
    private readonly WindowHandler      WindowHandler;

    private readonly ResourceLoader     ResourceLoader;
    private readonly ShaderHandler      ShaderHandler;

    public MirrorsEdgePlugin(IDalamudPluginInterface dalamud)
    {
        Version             = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        DalamudServices     = DalamudServices.Create(dalamud, this)!;

        MirrorServices      = new MirrorServices(DalamudServices);

        DirectXData         = new DirectXData(MirrorServices);

        ResourceLoader      = new ResourceLoader(DirectXData);

        ShaderHandler       = new ShaderHandler(MirrorServices, ResourceLoader, DirectXData);

        ResourceHandler     = new ResourceHandler(DalamudServices, MirrorServices);

        HookManager         = new HookManager(DalamudServices, MirrorServices, DirectXData, ShaderHandler, ResourceHandler);

        CameraHandler       = new CameraHandler(DalamudServices, MirrorServices, HookManager.CameraHooks);

        WindowHandler       = new WindowHandler(DalamudServices, MirrorServices, CameraHandler, HookManager.RendererHook, HookManager.ScreenHook, ShaderHandler, DirectXData, HookManager.BackBufferHook);
    }

    public void Dispose()
    {
        ResourceHandler.Dispose();

        HookManager.Dispose();
        WindowHandler.Dispose();
        CameraHandler.Dispose();

        DirectXData.Dispose();
    }
}
