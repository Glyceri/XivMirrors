using Dalamud.Plugin;
using MirrorsEdge.XIVMirrors.Services;
using System.Reflection;
using System.Threading;

namespace MirrorsEdge;

public sealed class MirrorsEdgePlugin : IDalamudPlugin
{
    //if you need to compile shaders at runtime and want to handle it gracefully, catch DllNotFoundException/EntryPointNotFoundException and tell the user to winetricks d3dcompiler_47 (assuming it's 47 you use)

    public readonly string Version;

    private MirrorsEdgeStarter? mirrorsEdgeStarter;

    public MirrorsEdgePlugin(IDalamudPluginInterface dalamud)
    {
        Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown Version";

        DalamudServices dalamudServices = DalamudServices.Create(dalamud, this);

        // This NEEDS to start on a framework thread. There really is no other way around it.
        // Hooking in the middle of a drawcall is just detrimental istg.
        _ = dalamudServices.Framework.RunOnFrameworkThread(() =>
        {
            mirrorsEdgeStarter = new MirrorsEdgeStarter(dalamudServices);
        });
    }

    public void Dispose()
    {
        mirrorsEdgeStarter?.Dispose();
    }
}
