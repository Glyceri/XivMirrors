using Dalamud.Plugin.Services;
using System;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.Wrappers;

internal class MirrorLog : IMirrorLog
{
    private readonly IPluginLog    PluginLog;
    private readonly Configuration Configuration;
    
    public MirrorLog(IPluginLog pluginLog, Configuration configuration)
    {
        PluginLog     = pluginLog;
        Configuration = configuration;
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

    public void LogExtremelyVerbose(object? message)
    {
        if (!Configuration.ExtremelyVerbose)
        {
            return;
        }
        
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