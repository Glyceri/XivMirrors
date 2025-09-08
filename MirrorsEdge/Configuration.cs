using Dalamud.Configuration;
using System;

namespace MirrorsEdge;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        
    }
}
