using System;

namespace MirrorsEdge.XIVMirrors.Commands.Interface;

internal interface ICommand : IDisposable
{
    string CommandCode { get; }
    string Description { get; }
    bool   ShowInHelp  { get; }

    void OnCommand(string command, string args);
}
