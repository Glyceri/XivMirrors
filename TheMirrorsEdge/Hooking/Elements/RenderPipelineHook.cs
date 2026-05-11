using Dalamud.Utility.Signatures;
using System;
using System.Runtime.InteropServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SharpDX.Direct3D11;
using TheMirrorsEdge;
using TheMirrorsEdge.CSClone;
using TheMirrorsEdge.Hooking;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using Device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

public unsafe class RenderPipelineInjector : HookableElement
{
    //Client::Graphics::Kernel::Context_PushBackCommand
    
    private delegate int  OMPresentDelegate(nint swapChain, uint syncInterval, uint flags);
    private readonly Hook<OMPresentDelegate>? OMPresentHook;
    
    private delegate void PushbackDg(nint a, nint b);
    [Signature("E8 ?? ?? ?? ?? 0F 28 B4 24 A0 01 00 00 48 8B 8C 24 90 01 00 00", Fallibility = Fallibility.Fallible)]
    private PushbackDg? PushbackFn = null;

    private delegate nint AllocateQueueMemoryDg(nint a, ulong b);
    [Signature("E8 ?? ?? ?? ?? 48 8B F8 48 85 C0 0f 84 ?? ?? ?? ?? 45 33 C0 41 BA 05 00 00 00", Fallibility = Fallibility.Fallible)]
    private AllocateQueueMemoryDg? AllocateQueueMemmoryFn = null;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate nint GetThreadedDataDg();
    GetThreadedDataDg GetThreadedDataFn;

    public const uint PAGE_EXECUTE_READWRITE = 0x40;

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool VirtualProtect(
        void* lpAddress,
        nuint dwSize,
        uint flNewProtect,
        out uint lpflOldProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool FlushInstructionCache(
        nint hProcess,
        void* lpBaseAddress,
        nuint dwSize);
    
    [DllImport("kernel32.dll")]
    public static extern nint GetCurrentProcess();
    
    private GCHandle getThreadedDataHandle;
    byte[] GetThreadedDataASM =
        {
                0x55, // push rbp
                0x65, 0x48, 0x8B, 0x04, 0x25, 0x58, 0x00, 0x00, 0x00, // mov rax,gs:[00000058]
                0x5D, // pop rbp
                0xC3  // ret
            };
    private nint tls_index;

    private const string g_tls_index = "8B 0D ?? ?? ?? ?? 45 33 E4 41";

    public RenderPipelineInjector(DalamudServices dalamudServices, MirrorServices mirrorServices)
        : base(dalamudServices, mirrorServices)
    {
        OMPresentHook = GetHook<OMPresentDelegate>(MirrorServices.DirectXData.SwapChain.NativePointer, 0, 8, OMPresentDetour);
        
        tls_index = DalamudServices.SigScanner.GetStaticAddressFromSig(g_tls_index);
        getThreadedDataHandle = GCHandle.Alloc(GetThreadedDataASM, GCHandleType.Pinned);
        var processHandle = GetCurrentProcess();
        if (!VirtualProtect((void*)getThreadedDataHandle.AddrOfPinnedObject(), (UIntPtr)GetThreadedDataASM.Length, PAGE_EXECUTE_READWRITE, out uint _))
        {
            throw new Exception("Failed to VirtualProtectEx");
        }
        
        if (!FlushInstructionCache(processHandle, (void*)getThreadedDataHandle.AddrOfPinnedObject(), (UIntPtr)GetThreadedDataASM.Length))
        {
            throw new Exception("Failed to FlushInstructionCache");
        }

        GetThreadedDataFn = Marshal.GetDelegateForFunctionPointer<GetThreadedDataDg>(getThreadedDataHandle.AddrOfPinnedObject());
        
    }

    public nint GetThreadedOffset()
    {
        nint threadedData = GetThreadedDataFn();
        if (threadedData != 0)
        {
            threadedData = *(nint*)(threadedData + (nint)((*(int*)tls_index) * 8));
            threadedData = *(nint*)(threadedData + 0x238);
        }
        return threadedData;
    }
    
    private delegate nint PushbackUIDelegate(nint a1, char a2);
    
    [Signature("E8 ?? ?? ?? ?? EB ?? E8 ?? ?? ?? ?? 4C 8D 5C 24 50", DetourName = nameof(PushbackUIDetour))]
    private Hook<PushbackUIDelegate>? PushbackUIHook = null;
    

    private delegate void RenderThreadSetRenderTargetDelegate(Device* deviceInstance, SetRenderTargetCommand* command);
    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 5F 18", DetourName = nameof(RenderThreadSetRenderTargetDetour))]
    private Hook<RenderThreadSetRenderTargetDelegate>? RenderThreadSetRenderTargetHook = null;

    public MappedTexture? MappedTexture  = null;
    public MappedTexture? MappedTexture2 = null;
    public MappedTexture? MappedTexture3 = null;
    
    private void RenderThreadSetRenderTargetDetour(Device* deviceInstance, SetRenderTargetCommand* command)
    {
        try
        {
            
            var renderTargets = command->numRenderTargets;
            
            if (renderTargets == 201 || renderTargets == 202)
            {
                if (renderTargets == 201)
                {
                    // TODO: COPY TEXTURE
                    MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();
                    
                    if (renderTargetManager == null)
                    {
                        return;
                    }
                    
                    Texture* backBuffer = renderTargetManager->BackBuffer;
                    
                    if (backBuffer == null)
                    {
                        return;
                    }
                    
                    MappedTexture3?.Dispose();
                    MappedTexture3 = null;
                    
                    MappedTexture3 = MappedTexture.CloneFrom(backBuffer, MirrorServices.DirectXData);
                }
                
            }
            else
            {
                RenderThreadSetRenderTargetHook!.OriginalDisposeSafe(deviceInstance, command);
            }
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }
    
    public override void Init()
    {
        OMPresentHook?.Enable();
        PushbackUIHook?.Enable();
        RenderThreadSetRenderTargetHook?.Enable();
    }

    public override void Dispose()
    {
        OMPresentHook?.Dispose();
        PushbackUIHook?.Dispose();
        RenderThreadSetRenderTargetHook?.Dispose();
        
        MappedTexture?.Dispose();
        MappedTexture2?.Dispose();
        MappedTexture3?.Dispose();
    }
    
    private RenderPhase _currentRenderPhase = RenderPhase.GamePhase;
    
    
    private int OMPresentDetour(nint swapChain, uint syncInterval, uint flags)
    {
        MirrorServices.MirrorLog.LogVerbose("OMPresentDetour");
        
        int returner = OMPresentHook!.Original(swapChain, syncInterval, flags);

        try
        {
            
            Texture2D texture2D = MirrorServices.DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);
            
            if (_currentRenderPhase == RenderPhase.MirrorPhase)
            {
                /*
                MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();
                    
                if (renderTargetManager == null)
                {
                    return returner;
                }
                    
                Texture* backBuffer = renderTargetManager->BackBuffer;
                    
                if (backBuffer == null)
                {
                    return returner;
                }
                    
                MappedTexture2?.Dispose();
                MappedTexture2 = null;
                    
                MappedTexture2 = MappedTexture.CloneFrom(backBuffer, MirrorServices.DirectXData);
                */
                
                
                
                MappedTexture2?.Dispose();
                MappedTexture2 = null;
                    
                MappedTexture2 = MappedTexture.CloneFrom(texture2D, MirrorServices.DirectXData);
            }
            else
            {
                            
                MappedTexture?.Dispose();
                MappedTexture = null;
                    
                MappedTexture = MappedTexture.CloneFrom(texture2D, MirrorServices.DirectXData);
            }
            
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
        
        return returner;
    }
    
    public void QueueRenderTargetCommand(RenderPhase renderPhase)
    {
        nint threadedOffset = GetThreadedOffset();
        
        if (threadedOffset == 0)
        {
            return;
        }
        
        SetRenderTargetCommand* queueData = (SetRenderTargetCommand*)AllocateQueueMemmoryFn!(threadedOffset, (ulong)sizeof(SetRenderTargetCommand));
        if (queueData == null)
        {
            return;
        }
        
        *queueData = new SetRenderTargetCommand();
        queueData->SwitchType = 0;
        queueData->numRenderTargets = renderPhase == RenderPhase.GamePhase ? 201 : 202 ;
        queueData->RenderTarget0 = null;
        PushbackFn!(threadedOffset, (nint)queueData);
    }
    
    private nint PushbackUIDetour(nint a1, char a2)
    {
        try
        {
            if (_currentRenderPhase == RenderPhase.GamePhase)
            {
                _currentRenderPhase = RenderPhase.MirrorPhase;
            }
            else
            {
                _currentRenderPhase = RenderPhase.GamePhase;
            }
            
            MirrorServices.MirrorLog.LogVerbose($"Pushing back UI! [{a1}, {(int)a2}]");
            
            //if (MirrorServices.UserList.LocalPlayer != null)
            {
                QueueRenderTargetCommand(_currentRenderPhase);
                
                if (_currentRenderPhase == RenderPhase.MirrorPhase)
                {
                    QueueClearCommand();
                }
            }
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
        
        return PushbackUIHook!.Original(a1, a2);;
    }
    
    public void QueueClearCommand()
    {
        bool depth = false;
        float r = 0;
        float g = 0;
        float b = 0;
        float a = 0;
        nint threadedOffset = GetThreadedOffset();
        if (threadedOffset != 0)
        {
            nint queueData = AllocateQueueMemmoryFn!(threadedOffset, (ulong)sizeof(ClearCommand));
            if (queueData != 0)
            {
                ClearCommand* cmd = (ClearCommand*)queueData;
                *cmd = new ClearCommand();
                cmd->SwitchType = 4;
                cmd->clearType = ((depth) ? 7 : 1);
                cmd->colorR = r;
                cmd->colorG = g;
                cmd->colorB = b;
                cmd->colorA = a;
                cmd->clearDepth = 1;
                cmd->clearStencil = 0;
                cmd->clearCheck = 0;
                PushbackFn!(threadedOffset, queueData);
            }
        }
    }
}

public enum RenderPhase
{
    MirrorPhase,
    GamePhase,
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ClearCommand
{
    [FieldOffset(0x00)] public int SwitchType;
    [FieldOffset(0x04)] public int clearType;
    [FieldOffset(0x08)] public float colorB;
    [FieldOffset(0x0C)] public float colorG;
    [FieldOffset(0x10)] public float colorR;
    [FieldOffset(0x14)] public float colorA;
    [FieldOffset(0x18)] public float clearDepth;
    [FieldOffset(0x1C)] public int clearStencil;
    [FieldOffset(0x20)] public int clearCheck;
    [FieldOffset(0x24)] public float Top;
    [FieldOffset(0x28)] public float Left;
    [FieldOffset(0x2C)] public float Width;
    [FieldOffset(0x30)] public float Height;
    [FieldOffset(0x34)] public float MinZ;
    [FieldOffset(0x38)] public float MaxZ;
};

[StructLayout(LayoutKind.Explicit, Size = 0x38)]
public unsafe struct SetRenderTargetCommand
{
    [FieldOffset(0x00)] public int SwitchType;
    [FieldOffset(0x04)] public int numRenderTargets;
    [FieldOffset(0x08)] public Texture* RenderTarget0;
    [FieldOffset(0x10)] public Texture* RenderTarget1;
    [FieldOffset(0x18)] public Texture* RenderTarget2;
    [FieldOffset(0x20)] public Texture* RenderTarget3;
    [FieldOffset(0x28)] public Texture* RenderTarget4;
    [FieldOffset(0x30)] public Texture* DepthBuffer;
};