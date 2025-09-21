using System;
using System.Collections.Generic;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Windowing;
using MirrorsEdge.XIVMirrors.Commands.Interface;
using MirrorsEdge.XIVMirrors.Commands.Commands;

namespace MirrorsEdge.XIVMirrors.Commands;

internal class CommandHandler : IDisposable
{
    private readonly List<ICommand> commands = new List<ICommand>();

    public CommandHandler(DalamudServices dalamudServices, MirrorServices mirrorServices, WindowHandler windowHandler)
    {
        RegisterCommand(new DevCommand(dalamudServices, mirrorServices, windowHandler));
    }

    private void RegisterCommand(ICommand command)
    {
        commands.Add(command);
    }

    public void Dispose()
    {
        foreach (ICommand command in commands)
        {
            command?.Dispose();
        }

        commands.Clear();
    }
}
