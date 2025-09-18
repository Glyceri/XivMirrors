using MirrorsEdge.XIVMirrors.Services.Interfaces;
using MirrorsEdge.XIVMirrors.Services.Wrappers;

namespace MirrorsEdge.XIVMirrors.Services;

internal class MirrorServices
{
    private readonly DalamudServices DalamudServices;

    public readonly Configuration   Configuration;
    public readonly IMirrorLog      MirrorLog;
    public readonly Utils           Utils;

    public MirrorServices(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;

        Configuration   = DalamudServices.DalamudPlugin.GetPluginConfig() as Configuration ?? new Configuration();

        Configuration.Initialise(DalamudServices.DalamudPlugin);

        MirrorLog       = new MirrorLog(DalamudServices.PluginLog);

        Utils           = new Utils();
    }
}
