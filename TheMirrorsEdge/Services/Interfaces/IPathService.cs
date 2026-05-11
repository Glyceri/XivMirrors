using TheMirrorsEdge.Services.Enums;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IPathService
{
    public const uint MaxPathLength = 100;
    
    bool   PluginPathValid { get; }
    string PluginPath      { get; }
    
    FilePathState TrySetNewPluginPath(string path);
    FilePathState CalculateValidity(string path);
    string? GetErrorMessage(FilePathState filePathState);
}