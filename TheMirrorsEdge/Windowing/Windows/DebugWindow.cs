using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;
using SharpDX;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;

namespace TheMirrorsEdge.Windowing.Windows;

public class DebugWindow : Window, IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    private readonly HookHandler     HookHandler;
    private readonly ShaderHandler   ShaderHandler;
    
    protected System.Numerics.Vector2 MinSize      { get; } = new System.Numerics.Vector2(350, 136);
    protected System.Numerics.Vector2 MaxSize      { get; } = new System.Numerics.Vector2(3000, 3000);
    protected System.Numerics.Vector2 DefaultSize  { get; } = new System.Numerics.Vector2(800, 400);
    
    public DebugWindow(DalamudServices dalamudServices, MirrorServices mirrorServices, HookHandler hookHandler, ShaderHandler shaderHandler) 
        : base("DebugWindow", ImGuiWindowFlags.None, true)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
        HookHandler     = hookHandler;
        ShaderHandler   = shaderHandler;
        
        SizeCondition = ImGuiCond.FirstUseEver;
        Size = DefaultSize;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = MinSize,
            MaximumSize = MaxSize,
        };
        
        IsOpen = true;
    }

    private readonly List<RenderTexture> RenderTextures = [];
    private readonly List<MappedTexture> MappedTextures = [];
    
    private void HandleRenderTarget(RenderTexture renderTarget, Vector4 colour)
    {
        RenderTextures.Add(renderTarget);
        
        MappedTexture mappedTexture = renderTarget.ToMappedTexture(MirrorServices.DirectXData);
        
        MappedTextures.Add(mappedTexture);
        
        ShaderHandler.ChannelMappedShader.Bind(mappedTexture, colour, renderTarget);
        
        ShaderHandler.ChannelMappedShader.Draw();
        
        ShaderHandler.ChannelMappedShader.UnbindTexture();
    }
    

    
    public override void Draw()
    {
        int counter = 0;
        
        foreach (RenderTexture mappedTexture in HookHandler.RenderHook.RenderTextures)
        {
            ImGui.ImageButton(mappedTexture.Handle, new System.Numerics.Vector2(200, 200));
            
            ImGui.SameLine();
            
            counter ++;
            
            if (counter >= 4)
            {
                counter = 0;
                
                ImGui.NewLine();
            }
        }
        
        ImGui.NewLine();
        
        counter = 0;
        
        foreach (MappedTexture mappedTexture in HookHandler.RenderHook.MappedTextures)
        {
            ImGui.ImageButton(mappedTexture.Handle, new System.Numerics.Vector2(200, 200));
            
            ImGui.SameLine();
            
            counter ++;
            
            if (counter >= 5)
            {
                counter = 0;
                
                ImGui.NewLine();
            }
        }
        
        foreach (RenderTexture renderTarget in RenderTextures)
        {
            renderTarget.Dispose();
        }
        
        RenderTextures.Clear();
        
        foreach (MappedTexture renderTarget in MappedTextures)
        {
            renderTarget.Dispose();
        }
        
        MappedTextures.Clear();
    }

    public void Dispose()
    {
        foreach (RenderTexture renderTarget in RenderTextures)
        {
            renderTarget.Dispose();
        }
        
        RenderTextures.Clear();
        
        foreach (MappedTexture renderTarget in MappedTextures)
        {
            renderTarget.Dispose();
        }
        
        MappedTextures.Clear();
    }
}