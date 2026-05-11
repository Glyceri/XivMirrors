using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace TheMirrorsEdge;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public bool ExtremelyVerbose = false;
    
    public string PluginPath = string.Empty;
    
    public void Save(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.SavePluginConfig(this);
    }
}
