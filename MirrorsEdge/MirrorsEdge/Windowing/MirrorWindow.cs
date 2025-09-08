using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Windowing.Interfaces;
using System.Numerics;

namespace MirrorsEdge.MirrorsEdge.Windowing;

internal abstract class MirrorWindow : Window, IMirrorWindow
{
    protected abstract Vector2 MinSize      { get; }
    protected abstract Vector2 MaxSize      { get; }
    protected abstract Vector2 DefaultSize  { get; }

    protected readonly DalamudServices  DalamudServices;
    protected readonly MirrorServices   MirrorServices;
    protected readonly WindowHandler    WindowHandler;

    protected MirrorWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None) : base(name, flags, true)
    {
        WindowHandler   = windowHandler;
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;

        SizeCondition = ImGuiCond.FirstUseEver;
        Size = DefaultSize;

        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = MinSize,
            MaximumSize = MaxSize,
        };
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    protected virtual void OnEarlyDraw() { }
    protected virtual void OnDraw() { }
    protected virtual void OnLateDraw() { }
    protected virtual void OnDirty() { }
    protected virtual void OnModeChange() { }
    protected virtual void OnDispose() { }

    private readonly Vector2 windowPadding      = new(8, 8);
    private readonly Vector2 framePadding       = new(4, 3);
    private readonly Vector2 itemInnerSpacing   = new(4, 4);
    private readonly Vector2 itemSpacing        = new(4, 4);

    public sealed override void PreDraw()
    {
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding,     windowPadding    * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,      framePadding     * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,       itemSpacing      * ImGuiHelpers.GlobalScale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing,  itemInnerSpacing * ImGuiHelpers.GlobalScale);

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
    {
        OnDispose();
    }
}
