using System.Numerics;
using Dalamud.Bindings.ImGui;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Windowing.Windows;

public class ConfigurationWindow : MirrorWindow
{
    protected override Vector2  MinSize     { get; } = new Vector2(400, 200);
    protected override Vector2  MaxSize     { get; } = new Vector2(400, 1200);
    protected override Vector2  DefaultSize { get; } = new Vector2(400, 500);
    
    public ConfigurationWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Configuration")
    {
        DalamudServices.DalamudPlugin.UiBuilder.OpenConfigUi += Open;
    }

    protected override void OnDraw()
    {
        if (ImGui.Checkbox("Very Verbose Logging", ref MirrorServices.Configuration.ExtremelyVerbose))
        {
            MirrorServices.Configuration.Save(DalamudServices.DalamudPlugin);
        }
    }

    protected override void OnDispose()
    {
        DalamudServices.DalamudPlugin.UiBuilder.OpenConfigUi -= Open;
    }
}