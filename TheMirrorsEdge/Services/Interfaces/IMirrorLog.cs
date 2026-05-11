using System;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IMirrorLog
{
    void Log(object? message);
    void LogInfo(object? obj);
    void LogWarning(object? obj);
    void LogFatal(object? obj);
    void LogVerbose(object? obj);
    void LogError(Exception e, object? obj);
    void LogException(Exception e);
}