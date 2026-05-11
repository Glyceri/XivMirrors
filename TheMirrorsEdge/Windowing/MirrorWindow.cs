using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Windowing;

public abstract class MirrorWindow : Window, IDisposable
{
    private static readonly Vector2 windowPadding    = new Vector2(8, 8);
    private static readonly Vector2 framePadding     = new Vector2(4, 3);
    private static readonly Vector2 itemInnerSpacing = new Vector2(4, 4);
    private static readonly Vector2 itemSpacing      = new Vector2(4, 4);
    
    protected abstract Vector2 MinSize     { get; }
    protected abstract Vector2 MaxSize     { get; }
    protected abstract Vector2 DefaultSize { get; }
    
    protected readonly DalamudServices DalamudServices;
    protected readonly WindowHandler   WindowHandler;
    protected readonly MirrorServices  MirrorServices;
    
    private float lastGlobalScale = 0;
    
    protected MirrorWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, string name, ImGuiWindowFlags windowFlags = ImGuiWindowFlags.None) 
        : base(name, windowFlags, true)
    {
        WindowHandler   = windowHandler;
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;

        SizeCondition   = ImGuiCond.FirstUseEver;
        Size            = DefaultSize;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = MinSize,
            MaximumSize = MaxSize,
        };
    }
    
    protected virtual void OnEarlyDraw()  { }
    protected virtual void OnDraw()       { }
    protected virtual void OnLateDraw()   { }
    protected virtual void OnDispose()    { }
    
    public void Close() 
        => IsOpen = false;

    public void Open()
        => IsOpen = true;
    
    public sealed override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,    windowPadding    * WindowHandler.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,     framePadding     * WindowHandler.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,      itemSpacing      * WindowHandler.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, itemInnerSpacing * WindowHandler.GlobalScale);

        float currentGlobalScale = WindowHandler.FontScale;

        if (lastGlobalScale != currentGlobalScale)
        {
            lastGlobalScale = currentGlobalScale;

            SizeCondition   = ImGuiCond.FirstUseEver;
            Size            = DefaultSize * currentGlobalScale;

            SizeConstraints = new WindowSizeConstraints()
            {
                MinimumSize = MinSize * currentGlobalScale,
                MaximumSize = MaxSize * currentGlobalScale,
            };
        }

        OnEarlyDraw();
    }
    
    public sealed override void PostDraw()
    {
        OnLateDraw();

        ImGui.PopStyleVar(4);
    }

    public sealed override void Draw()
    {
        OnDraw();
    }
    
    public void Dispose() 
        => OnDispose();
}