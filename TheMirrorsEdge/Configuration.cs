using Dalamud.Configuration;
using System;
using Dalamud.Plugin;

namespace TheMirrorsEdge;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public bool ExtremelyVerbose = false;
    
    public void Save(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
    }
}
