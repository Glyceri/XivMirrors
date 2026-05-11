using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Interface.ImGuiFileDialog;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class FileDialogService : IFileDialogService
{
    private readonly FileDialogManager FileDialogManager;
    
    private bool _isOpen = false;
    
    public FileDialogService()
    {
        FileDialogManager = new FileDialogManager();
    }
    
    public void OpenFileDialogue(string title, string filters, Action<bool, List<string>> callback, int selectionCountMax, string? startPath, bool forceStartPath)
    {
        _isOpen = true;
        
        FileDialogManager.OpenFileDialog(title, filters, CreateCallback(callback), selectionCountMax, startPath?? Directory.GetCurrentDirectory());
    }
    
    public void OpenFolderDialogue(string title, Action<bool, string> callback, string? startPath)
    {
        _isOpen = true;
        
        FileDialogManager.OpenFolderDialog(title, CreateCallback(callback), startPath ?? Directory.GetCurrentDirectory(), true);
    }
    
    public void Draw()
    {
        if (!_isOpen)
        {
            return;
        }
        
        FileDialogManager.Draw();
    }
    
    public void Close()
        => _isOpen = false;
    
    public Action<bool, List<string>> CreateCallback(Action<bool, List<string>> callback) 
        => (valid, list) =>
        {
            _isOpen = false;
            
            callback(valid, list);
        };
    
    public Action<bool, string> CreateCallback(Action<bool, string> callback)
        => (valid, value) =>
        {
            _isOpen = false;
            
            callback(valid, value);
        };
}