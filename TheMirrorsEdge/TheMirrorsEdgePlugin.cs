using Dalamud.Plugin;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge;

public sealed class TheMirrorsEdgePlugin : IDalamudPlugin
{
    private readonly DalamudServices DalamudServices;
    
    private TheMirrorsEdgeStarter? TheMirrorsEdgeStarter;
    
    public static bool DISPOSED = false;
    
    public TheMirrorsEdgePlugin(IDalamudPluginInterface pluginInterface)
    {
        DalamudServices = DalamudServices.Create(pluginInterface, this);
        
        // This NEEDS to start on a framework thread. There really is no other way around it.
        // Hooking in the middle of a drawcall is just detrimental istg.
        _ = DalamudServices.Framework.RunOnFrameworkThread(() =>
        {
            TheMirrorsEdgeStarter = new TheMirrorsEdgeStarter(DalamudServices);
        });
    }

    public void Dispose()
    {
        DISPOSED = true;
        
        TheMirrorsEdgeStarter?.Dispose();
    }
}
