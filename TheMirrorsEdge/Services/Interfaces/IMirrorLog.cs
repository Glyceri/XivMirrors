using System;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IMirrorLog
{
    void Log(object? message);
    void LogInfo(object? message);
    void LogWarning(object? message);
    void LogFatal(object? message);
    void LogVerbose(object? message);
    void LogExtremelyVerbose(object? message);
    void LogError(Exception e, object? message);
    void LogException(Exception e);
}