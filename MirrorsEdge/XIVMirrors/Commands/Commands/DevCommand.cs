using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Windowing;
using MirrorsEdge.XIVMirrors.Windowing.Windows;

namespace MirrorsEdge.XIVMirrors.Commands.Commands;

internal class DevCommand(DalamudServices dalamudServices, MirrorServices mirrorServices, WindowHandler windowHandler) : Command(dalamudServices, mirrorServices, windowHandler)
{
    public override string CommandCode { get; } = "/mirrordev";
    public override string Description { get; } = "Toggles the Mirror Dev Window.";
    public override bool   ShowInHelp  { get; } = false;

    public override void OnCommand(string command, string args)
        => WindowHandler.Toggle<DebugWindow>();
}
