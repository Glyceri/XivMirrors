using MirrorsEdge.XIVMirrors.Services.Interfaces;
using MirrorsEdge.XIVMirrors.Services.Wrappers;

namespace MirrorsEdge.XIVMirrors.Services;

internal class MirrorServices
{
    private readonly DalamudServices DalamudServices;

    public readonly IMirrorLog MirrorLog;

    public MirrorServices(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;

        MirrorLog = new MirrorLog(DalamudServices.PluginLog);
    }
}
