using System;
using System.IO;
using Dalamud.Utility;
using TheMirrorsEdge.Services.Enums;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class PathService : IPathService
{
    private readonly Configuration   Configuration;
    private readonly IMirrorLog      MirrorLog;
    private readonly DalamudServices DalamudServices;
    
    private string lastPath         = string.Empty;
    private bool   _pluginPathValid = false;
    
    private FilePathState filePathState = FilePathState.Error;
    private string lastException = "NO ERROR FOUND";
    
    public PathService(DalamudServices dalamudServices, IMirrorLog mirrorLog, Configuration configuration)
    {
        DalamudServices = dalamudServices;
        MirrorLog       = mirrorLog;
        Configuration   = configuration;
        
        filePathState   = TrySetNewPluginPath(configuration.PluginPath);
    }
    
    public string PluginPath
        => Configuration.PluginPath;
    
    public bool PluginPathValid 
        => _pluginPathValid;
    
    public FilePathState TrySetNewPluginPath(string path)
    {
        _pluginPathValid = false;
        
        Configuration.PluginPath = path;
        
        filePathState    = CalculateValidity(path);
        
        lastPath         = path;
        
        if (filePathState == FilePathState.Valid)
        {
            _pluginPathValid = true;
            
            Configuration.Save(DalamudServices.DalamudPlugin);
        }
        
        return filePathState;
    }

    private bool IsSubPathOf(string basePath, string subPath)
    {
        if (basePath.Length == 0)
        {
            return false;
        }

        string rel = Path.GetRelativePath(basePath, subPath);
        
        return (rel == "." || !rel.StartsWith('.') && !Path.IsPathRooted(rel));
    }
    
    public FilePathState CalculateValidity(string path)
    {
        if (path.IsNullOrWhitespace())
        {
            return FilePathState.NoPathSelected;
        }
        
        if (path.Length >= IPathService.MaxPathLength)
        {
            return FilePathState.Size;
        }
        
        try
        {
            if (!Directory.Exists(path))
            {
                return FilePathState.NotADirectory;
            }
            
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            if (IsSubPathOf(desktop, path))
            {
                return FilePathState.Desktop;
            }
            
            string programFiles    = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            
            if (IsSubPathOf(path, programFiles) || IsSubPathOf(path, programFilesX86))
            {
                return FilePathState.ProgramFiles;
            }
            
            DirectoryInfo dalamudPath = DalamudServices.DalamudPlugin.ConfigDirectory.Parent!.Parent!;
            
            if (IsSubPathOf(dalamudPath.FullName, path))
            {
                return FilePathState.Dalamud;
            }
        }
        catch  (Exception e)
        {
            lastException = e.Message;
            
            MirrorLog.LogException(e);   
            
            return FilePathState.Error;
        }

        return FilePathState.Valid;
    }
    
    public string? GetErrorMessage(FilePathState filePathState)
    {
        switch (filePathState)
        {
            case FilePathState.Error:          return $"An error has occured whils't selecting a path: [{lastException}]";
            case FilePathState.NotADirectory:  return $"The path '{lastPath}' does not exist or is not a directory.";
            case FilePathState.NoPathSelected: return $"The path is empty.";
            case FilePathState.Desktop:        return $"Path cannot be part of the Desktop.";
            case FilePathState.ProgramFiles:   return $"Path cannot be part of the Program Files.";
            case FilePathState.Dalamud:        return $"Path cannot be part of the Dalamud.";
            case FilePathState.Size:           return $"Path is too big.";
        }
        
        return null;
    }
}