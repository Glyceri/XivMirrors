using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Services.ChildServices;
using TheMirrorsEdge.Services.Interfaces;
using TheMirrorsEdge.Services.Wrappers;

namespace TheMirrorsEdge.Services;

public class MirrorServices
{
    private readonly DalamudServices  DalamudServices;
    
    public readonly  Configuration      Configuration;
    public readonly  IMirrorLog         MirrorLog;
    public readonly  DirectXData        DirectXData;
    public readonly  IFileDialogService FileDialogService;
    public readonly  IPathService       PathService;
    public readonly  IUserList          UserList;
    public readonly  IResourceLoader    ResourceLoader;
    public readonly  IStatePreserver    StatePreserver;
    public readonly  PrimitiveFactory   PrimitiveFactory;
    public readonly  IRenderService     RenderService;
    public readonly  IScreenshotService ScreenshotService;
    
    public MirrorServices(DalamudServices dalamudServices)
    {
        DalamudServices     = dalamudServices;    
        
        Configuration       = DalamudServices.DalamudPlugin.GetPluginConfig() as Configuration ?? new Configuration();
        
        MirrorLog           = new MirrorLog(DalamudServices.PluginLog, Configuration);
        
        DirectXData         = new DirectXData();
        
        FileDialogService   = new FileDialogService();
        
        PathService         = new PathService(dalamudServices, MirrorLog, Configuration);
        
        UserList            = new UserList();
        
        ResourceLoader      = new ResourceLoader();
        
        StatePreserver      = new StatePreserver(DirectXData);
        
        PrimitiveFactory    = new PrimitiveFactory();
        
        RenderService       = new RenderService(MirrorLog);
        
        ScreenshotService   = new ScreenshotService();
    }
}