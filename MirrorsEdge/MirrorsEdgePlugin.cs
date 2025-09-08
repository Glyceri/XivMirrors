using Dalamud.Plugin;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Windowing;
using System.Reflection;

namespace MirrorsEdge;

public sealed class MirrorsEdgePlugin : IDalamudPlugin
{
    public readonly string Version;

    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;

    private readonly HookManager        HookManager;
    private readonly CameraHandler      CameraHandler;
    private readonly WindowHandler      WindowHandler;

    public MirrorsEdgePlugin(IDalamudPluginInterface dalamud)
    {
        Version             = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        DalamudServices     = DalamudServices.Create(dalamud, this)!;

        MirrorServices      = new MirrorServices(DalamudServices);

        HookManager         = new HookManager(DalamudServices, MirrorServices);

        CameraHandler       = new CameraHandler(DalamudServices, MirrorServices, HookManager.CameraHooks);

        WindowHandler       = new WindowHandler(DalamudServices, MirrorServices, CameraHandler, HookManager.TextureHooker);
    }

    public void Dispose()
    {
        HookManager.Dispose();
        CameraHandler.Dispose();
        WindowHandler.Dispose();
    }
}
