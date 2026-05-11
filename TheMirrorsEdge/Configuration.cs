using Dalamud.Configuration;
using System;

namespace TheMirrorsEdge;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    
    public void Save()
    {
        
    }
}
