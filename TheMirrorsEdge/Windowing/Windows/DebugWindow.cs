using System.Numerics;
using Dalamud.Bindings.ImGui;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Windowing.Windows;

public class DebugWindow : MirrorWindow
{
    protected override Vector2 MinSize     => new Vector2(350, 136);
    protected override Vector2 MaxSize     => new Vector2(3000, 3000);
    protected override Vector2 DefaultSize => new Vector2(800, 400);
    
    private ulong _drawCount = 0;
    
    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(windowHandler, dalamudServices, mirrorServices, "DebugWindow") 
    {
        MirrorServices.RenderService.RegisterRenderListener(OnRender);
        
        Open();
    }
    
    protected override void OnDispose()
        => MirrorServices.RenderService.DeregisterRenderListener(OnRender);
    
    protected override void OnDraw()
        => ImGui.Text(_drawCount.ToString());
    
    private void OnRender()
        => _drawCount++;
}