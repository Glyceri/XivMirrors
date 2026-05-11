using System;
using System.Collections.Generic;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IFileDialogService
{
    void OpenFileDialogue(string title, string filters, Action<bool, List<string>> callback, int selectionCountMax, string? startPath, bool forceStartPath);
    void OpenFolderDialogue(string title, Action<bool, string> callback, string? startPath);
    
    void Draw();
    void Close();
}