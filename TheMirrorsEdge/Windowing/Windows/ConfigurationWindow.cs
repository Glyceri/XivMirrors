using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.Enums;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Windowing.Windows;

public class ConfigurationWindow : MirrorWindow
{
    protected override Vector2  MinSize     { get; } = new Vector2(600, 200);
    protected override Vector2  MaxSize     { get; } = new Vector2(600, 1200);
    protected override Vector2  DefaultSize { get; } = new Vector2(600, 800);
    
    private string? currentPath    = string.Empty;
    private string? lastConfigPath = string.Empty;
    
    public ConfigurationWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Configuration")
    {
        currentPath = MirrorServices.PathService.PluginPath;
        
        DalamudServices.DalamudPlugin.UiBuilder.OpenConfigUi += Open;
        
        IsOpen = true;
    }

    protected override void OnDispose()
    {
        DalamudServices.DalamudPlugin.UiBuilder.OpenConfigUi -= Open;
    }
    
    protected override void OnDraw()
    {
        DrawPathSelector();
        
        if (ImGui.Checkbox("Very Verbose Logging", ref MirrorServices.Configuration.ExtremelyVerbose))
        {
            MirrorServices.Configuration.Save(DalamudServices.DalamudPlugin);
        }
    }
    
    private string? ErrorMessage  = null;
    private string? NewPathToSave = null;
    
    private void DrawPathBar()
    {
        if (lastConfigPath != MirrorServices.PathService.PluginPath)
        {
            lastConfigPath = MirrorServices.PathService.PluginPath;
            
            currentPath    = lastConfigPath;
        }
        
        if (currentPath == null)
        {
            currentPath = string.Empty;
        }
        
        if (!ImGui.InputTextWithHint("##mirrorsRootDirectory"u8, "(YOU MUST DO THIS) Enter a Root Directory here..."u8, ref currentPath, (int)IPathService.MaxPathLength, ImGuiInputTextFlags.EnterReturnsTrue))
        {
            return;
        }
        
        ErrorMessage  = null;
        NewPathToSave = null;
        
        FilePathState pathState = MirrorServices.PathService.CalculateValidity(currentPath);
        
        if (pathState != FilePathState.Valid)
        {
            ErrorMessage = MirrorServices.PathService.GetErrorMessage(pathState);
        }
        else
        {
            NewPathToSave = currentPath;
        }
    }
    
    private void DrawSaveButton()
    {
        if (NewPathToSave == null)
        {
            return;
        }
        
        if (NewPathToSave == MirrorServices.Configuration.PluginPath)
        {
            return;
        }
        
        using (ImRaii.PushFont(UiBuilder.IconFont));
        using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(0, 1, 0, 1)));
        
        if (!ImGui.Button(FontAwesomeIcon.Save.ToIconString() + "###ROOTPATHMIRRORSSAVE"))
        {
            return;
        }
            
        ErrorMessage  = MirrorServices.PathService.GetErrorMessage(MirrorServices.PathService.TrySetNewPluginPath(NewPathToSave));
        NewPathToSave = null;
    }
    
    private void DrawRootButton()
    {
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            if (ImGui.Button(FontAwesomeIcon.FolderOpen.ToIconString() + "###ROOTICONPATHMIRRORS"))
            {
                MirrorServices.FileDialogService.OpenFolderDialogue("Select a Mirrors root folder", (selected, path) =>
                {
                    MirrorServices.MirrorLog.LogInfo(selected + " : " + path); 
                    
                    NewPathToSave = path;
                }, lastConfigPath);
            }
        }
        
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.SetTooltip("Select a Root Path");
        }
    }
    
    private void DrawHelpTooltip()
    {
        if (!ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            return;
        }
        
        ImGui.SetTooltip("Mirrors will store it's files here.\nMake sure the filepath is unique and not often used.\nThink of paths like your downloads folder or the desktop, do not place them here.\nThe games folder and dalamuds folder are also not allowed.\nAgain do NOT put your folder here!");
    }
    
    private void DrawPathSelector()
    {
        bool pathIsValid = MirrorServices.PathService.PluginPathValid && (ErrorMessage == null);
        
        ImRaii.StyleDisposable? styleDisposable = null;
        ImRaii.ColorDisposable? colorDisposable = null;
        
        if (!pathIsValid)
        {
            colorDisposable = ImRaii.PushColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1));
            styleDisposable = ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1);
        }

        try
        {
            DrawPathBar();
        }
        finally
        {
            colorDisposable?.Dispose();
            styleDisposable?.Dispose();
        }
        
        ImGui.SameLine();
        
        DrawRootButton();
        
        ImGui.SameLine();
        
        using (ImRaii.Disabled())
        using (ImRaii.PushFont(UiBuilder.IconFont)) 
            ImGui.Text(FontAwesomeIcon.InfoCircle.ToIconString());
        
        DrawHelpTooltip();
        
        ImGui.SameLine();
        
        using (ImRaii.Disabled())
            ImGui.Text("Root Directory");
        
        DrawHelpTooltip();

        DrawSaveButton();
        
        if (!ErrorMessage.IsNullOrWhitespace())
        {
            using (ImRaii.Disabled())
            using (ImRaii.PushColor(ImGuiCol.Border, new Vector4(1, 0, 0, 1)))
            using (ImRaii.PushStyle(ImGuiStyleVar.FrameBorderSize, 1))
            using (ImRaii.PushColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1)))
            {
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                ImGui.InputText("###MIRRORSWARNING", ref ErrorMessage, 200, ImGuiInputTextFlags.ReadOnly);
            }
        }
        else
        {
            if (ImGui.Button("Open Directory###MIRRORSOPENDIRECTORY"))
            {
                Process.Start(new ProcessStartInfo(MirrorServices.PathService.PluginPath)
                {
                    UseShellExecute = true,
                });
            }
        }
    }
}