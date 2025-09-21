using MirrorsEdge.XIVMirrors.Commands.Interface;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Windowing;
using Dalamud.Game.Command;

namespace MirrorsEdge.XIVMirrors.Commands;

internal abstract class Command : ICommand
{
    public abstract string CommandCode { get; }
    public abstract string Description { get; }
    public abstract bool   ShowInHelp  { get; }

    public abstract void OnCommand(string command, string args);

    protected readonly DalamudServices DalamudServices;
    protected readonly MirrorServices  MirrorServices;
    protected readonly WindowHandler   WindowHandler;

    public Command(DalamudServices dalamudServices, MirrorServices mirrorServices, WindowHandler windowHandler)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        WindowHandler   = windowHandler;

        CommandInfo commandInfo = new CommandInfo(OnCommand)
        {
            HelpMessage = Description,
            ShowInHelp  = ShowInHelp,
        };

        bool addedCommand = DalamudServices.CommandManager.AddHandler(CommandCode, commandInfo);

        if (addedCommand)
        {
            return;
        }

        MirrorServices.MirrorLog.LogWarning($"Failed to add command: [{CommandCode}, {Description}].");
    }

    public void Dispose()
    {
        bool removedSuccessfully = DalamudServices.CommandManager.RemoveHandler(CommandCode);

        if (removedSuccessfully)
        {
            return;
        }

        MirrorServices.MirrorLog.LogWarning($"Failed to remove command: [{CommandCode}, {Description}].");
    }
}
