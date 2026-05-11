using TheMirrorsEdge.Camera;
using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.Screenshotting;
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
    private readonly CameraHandler   CameraHandler;
    
    public TheMirrorsEdgeStarter(DalamudServices dalamudServices)
    {
        DalamudServices = dalamudServices;
        
        MirrorServices  = new MirrorServices(DalamudServices);
        
        MirrorServices.MirrorLog.LogInfo($"===We're on the Mirrors Edge.===");
        
        ShaderHandler   = new ShaderHandler(MirrorServices);
        
        HookHandler     = new HookHandler(DalamudServices, MirrorServices);
        
        CameraHandler   = new CameraHandler(DalamudServices, MirrorServices, HookHandler.CameraHook!);
        
        WindowHandler   = new WindowHandler(DalamudServices, MirrorServices, HookHandler, CameraHandler);
        
        MirrorServices.MirrorLog.LogInfo($"===Load Complete===");
    }

    public void Dispose()
    {
        CameraHandler.Dispose();
        HookHandler.Dispose();
        WindowHandler.Dispose();
        ShaderHandler.Dispose();
    }
}