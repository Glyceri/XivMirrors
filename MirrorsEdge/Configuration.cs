using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace MirrorsEdge;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface? dalamudPlugin;

    public int Version { get; set; } = 0;

    // ------- DEBUG -------
    public bool DebugClearAlpha = false;

    public void Initialise(IDalamudPluginInterface dalamudPlugin)
    {
        this.dalamudPlugin = dalamudPlugin;
    }

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        dalamudPlugin?.SavePluginConfig(this);
    }
}
