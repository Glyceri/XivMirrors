using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;
using TheMirrorsEdge.Windowing;

namespace TheMirrorsEdge;

public class TheMirrorsEdgeStarter
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;
    private readonly HookHandler     HookHandler;
    private readonly WindowHandler   WindowHandler;
    private readonly ShaderHandler   ShaderHandler;
    
    public TheMirrorsEdgeStarter(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;
        
        MirrorServices  = new MirrorServices(DalamudServices);
        
        MirrorServices.MirrorLog.LogInfo($"===We're on the Mirrors Edge.===");
        
        ShaderHandler   = new ShaderHandler(MirrorServices);
        
        HookHandler     = new HookHandler(DalamudServices, MirrorServices, ShaderHandler);
        
        WindowHandler   = new WindowHandler(DalamudServices, MirrorServices, HookHandler, ShaderHandler);
        
        MirrorServices.MirrorLog.LogInfo($"===Load Complete===");
    }

    public void Dispose()
    {
        HookHandler.Dispose();
        WindowHandler.Dispose();
    }
}