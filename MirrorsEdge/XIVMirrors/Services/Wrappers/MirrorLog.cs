using Dalamud.Plugin.Services;
using MirrorsEdge.XIVMirrors.Services.Interfaces;
using System;

namespace MirrorsEdge.XIVMirrors.Services.Wrappers;

internal class MirrorLog : IMirrorLog
{
    public static IMirrorLog? Instance { get; private set; }

    private readonly IPluginLog PluginLog;

    public MirrorLog(IPluginLog pluginLog)
    {
        PluginLog   = pluginLog;
        Instance    = this;
    }

    public void Log(object? message)
    {
        if (message == null) 
        { 
            return;
        }

        PluginLog.Debug($"{message}");
    }

    public void LogError(Exception e, object? message)
    {
        if (message == null)
        {
            return;
        }

        PluginLog.Error($"{e} : {message}");
    }

    public void LogException(Exception e)
    {
        PluginLog.Error($"{e}");
    }

    public void LogFatal(object? message)
    {
        if (message == null)
        {
            return;
        }

        PluginLog.Fatal($"{message}");
    }

    public void LogInfo(object? message)
    {
        if (message == null)
        {
            return;
        }

        PluginLog.Info($"{message}");
    }

    public void LogVerbose(object? message)
    {
        if (message == null)
        {
            return;
        }

        PluginLog.Verbose($"{message}");
    }

    public void LogWarning(object? message)
    {
        if (message == null)
        {
            return;
        }

        PluginLog.Warning($"{message}");
    }
}
