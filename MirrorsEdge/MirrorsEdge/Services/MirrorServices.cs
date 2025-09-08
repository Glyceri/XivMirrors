using MirrorsEdge.MirrorsEdge.Services.Interfaces;
using MirrorsEdge.MirrorsEdge.Services.Wrappers;

namespace MirrorsEdge.MirrorsEdge.Services;

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
