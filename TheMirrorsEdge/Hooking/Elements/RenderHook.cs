using System;
using System.Collections.Generic;
using System.Threading;
using Dalamud.Hooking;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Lumina.Excel.Sheets;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using TheMirrorsEdge.CSClone;
using TheMirrorsEdge.Hooking.Interfaces;
using TheMirrorsEdge.Resources;
using TheMirrorsEdge.Resources.Buffers;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Resources.Textures;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Shaders;
using Buffer = System.Buffer;
using PixelShader = SharpDX.Direct3D11.PixelShader;
using VertexShader = SharpDX.Direct3D11.VertexShader;
using PrimitiveDeclaration = (TheMirrorsEdge.Resources.Structs.Vertex[] vertices, ushort[] indices);

namespace TheMirrorsEdge.Hooking.Elements;

public unsafe class RenderHook : HookableElement
{
    private delegate int  OMPresentDelegate(nint swapChain, uint syncInterval, uint flags);
    private delegate void OMSetRenderTargetsDelegate(nint device, uint numViews, nint* renderTargetViews, nint depthStencilView);
    
    private delegate void OMDrawIndexed(nint context, uint indexCount, uint startIndexLocation, int baseVertexLocation);
    private delegate void OMDraw(nint context, uint vertexCount, uint startVertexLocation);
    private delegate void OMDrawIndexedInstanced(nint context, uint indexCountPerInstance, uint instanceCount, uint startIndexLocation, int baseVertexLocation, uint startInstanceLocation);
    private delegate void OMDrawInstanced(nint context, uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation, uint startInstanceLocation);
    private delegate void OMDrawAuto(nint context);
    private delegate void OMDrawIndexedInstancedIndirect(nint context, nint bufferForArgs, uint alignedByteOffsetForArgs);
    private delegate void OMDrawInstancedIndirect(nint context, nint bufferForArgs, uint alignedByteOffsetForArgs);
    private delegate int  OMFinishCommandList(nint context, byte restoreContext, nint* ppCommandList);
    
    private delegate nint DrawGBuffersDelegate(nint a1, nint a2);
    
    private readonly Hook<OMDrawIndexed>?                   OMDrawIndexedHook;
    private readonly Hook<OMDraw>?                          OMDrawHook;
    private readonly Hook<OMDrawIndexedInstanced>?          OMDrawIndexedInstancedHook;
    private readonly Hook<OMDrawInstanced>?                 OMDrawInstancedHook;
    private readonly Hook<OMDrawAuto>?                      OMDrawAutoHook;
    private readonly Hook<OMDrawIndexedInstancedIndirect>?  OMDrawIndexedInstancedIndirectHook;
    private readonly Hook<OMDrawInstancedIndirect>?         OMDrawInstancedIndirectHook;
    private readonly Hook<OMFinishCommandList>?             OMFinishCommandListHook;
    
    [Signature("48 8B C4 48 89 50 ?? 53 56", DetourName = nameof(DrawGBuffersDetour))]
    private readonly Hook<DrawGBuffersDelegate>?            DrawGBuffersHook;
    
    private int OMDrawIndexedHookCount = 0;
    private int OMDrawHookCount = 0;
    private int OMDrawIndexedInstancedHookCount = 0;
    private int OMDrawInstancedHookCount = 0;
    private int OMDrawAutoHookCount = 0;
    private int OMDrawIndexedInstancedIndirectHookCount = 0;
    private int OMDrawInstancedIndirectHookCount = 0;
    
    private bool allowCount = true;
    
    private readonly Hook<OMPresentDelegate>?          OMPresentHook;
    private readonly Hook<OMSetRenderTargetsDelegate>? OmSetRenderTargetsHook;
    
    private readonly IScreenHook ScreenHook;
    
    private uint ScreenWidth;
    private uint ScreenHeight;
    
    private Dictionary<(nint, nint), int> PairCount =  new Dictionary<(nint, nint), int>();
    
    private readonly Lock LockObject = new Lock();
    
    public readonly Dictionary<nint, RenderTargetView> RenderTargetViews = new Dictionary<nint, RenderTargetView>();
    
    private nint lastSwapChain  = nint.Zero;
    private nint lastBackBuffer = nint.Zero;
    
    private readonly BasicModel CubeModel;
    private readonly CameraBuffer CameraBuffer;
    
    private delegate nint PushbackUIDelegate(nint a1, char a2);
    
    [Signature("E8 ?? ?? ?? ?? EB ?? E8 ?? ?? ?? ?? 4C 8D 5C 24 50", DetourName = nameof(PushbackUIDetour))]
    private Hook<PushbackUIDelegate>? PushbackUIHook = null;
    
    public MappedTexture? BeforeUITexture;
    public DepthTexture? BeforeUIDepthTexture;
    
    ulong presentCount = 0;
    
    Dictionary<nint, ulong> uniqueRtv = new Dictionary<IntPtr, ulong>();
    Dictionary<nint, ulong> uniqueRtvDsv = new Dictionary<IntPtr, ulong>();
    
    public List<MappedTexture> MappedTextures = [];
    
    nint lookupRTV = nint.Zero;
    
    private readonly RasterizerState   RasterizerState;
    private readonly DepthStencilState DepthStencilState;
    
    private readonly IDalamudTextureWrap TextureWrap;
    private readonly ShaderResourceView  TextureResourceView;
    
    private readonly ShaderHandler ShaderHandler;
    private readonly CameraHook CameraHook;
    
    public RenderHook(DalamudServices dalamudServices, MirrorServices mirrorServices, IScreenHook screenHook, CameraHook cameraHook, ShaderHandler shaderHandler)
        : base(dalamudServices, mirrorServices)
    {
        CameraHook = cameraHook;
        ShaderHandler = shaderHandler;
        
        PrimitiveDeclaration cube = mirrorServices.PrimitiveFactory.Cube();

        CubeModel       = new BasicModel(MirrorServices.DirectXData, ref cube);
        CameraBuffer    = new CameraBuffer(MirrorServices.DirectXData);
        
        ScreenHook = screenHook;
        
        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);
        
        OmSetRenderTargetsHook  = GetHook<OMSetRenderTargetsDelegate>(MirrorServices.DirectXData.Context.NativePointer, 0, 33, OMSetRenderTargetsDetour);

        OMPresentHook           = GetHook<OMPresentDelegate>(MirrorServices.DirectXData.SwapChain.NativePointer, 0, 8, OMPresentDetour);
        
        OMDrawIndexedHook                   = GetHook<OMDrawIndexed>(MirrorServices.DirectXData.Context.NativePointer, 0, 12, OMDrawIndexedDetour);
        OMDrawHook                          = GetHook<OMDraw>(MirrorServices.DirectXData.Context.NativePointer, 0, 13, OMDrawDetour);
        OMDrawIndexedInstancedHook          = GetHook<OMDrawIndexedInstanced>(MirrorServices.DirectXData.Context.NativePointer, 0, 20, OMDrawIndexedInstancedDetour);
        OMDrawInstancedHook                 = GetHook<OMDrawInstanced>(MirrorServices.DirectXData.Context.NativePointer, 0, 21, OMDrawInstancedDetour);
        OMDrawAutoHook                      = GetHook<OMDrawAuto>(MirrorServices.DirectXData.Context.NativePointer, 0, 38, OMDrawAutoDetour);
        OMDrawIndexedInstancedIndirectHook  = GetHook<OMDrawIndexedInstancedIndirect>(MirrorServices.DirectXData.Context.NativePointer, 0, 39, OMDrawIndexedInstancedIndirectDetour);
        OMDrawInstancedIndirectHook         = GetHook<OMDrawInstancedIndirect>(MirrorServices.DirectXData.Context.NativePointer, 0, 40, OMDrawInstancedIndirectDetour);
        
        
        
        OMFinishCommandListHook         = GetHook<OMFinishCommandList>(MirrorServices.DirectXData.Context.NativePointer, 0, 114, OMFinishCommandListDetour);
        
        TextureWrap     = DalamudServices.TextureProvider.GetFromFile("/home/amber/Git/XivPlugins/MirrorsEdge/MirrorsEdge/XIVMirrors/Shaders/Files/nightsky.png").RentAsync().Result;
        TextureResourceView = new ShaderResourceView((nint)TextureWrap.Handle.Handle);
        
        RasterizerStateDescription rsDesc = new RasterizerStateDescription
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.Back,
            IsFrontCounterClockwise = true // flip front face
        };

        RasterizerState = new RasterizerState(MirrorServices.DirectXData.Device, rsDesc);

        DepthStencilStateDescription dsDesc = new DepthStencilStateDescription
        {
            IsDepthEnabled  = true,
            DepthWriteMask  = DepthWriteMask.All,
            DepthComparison = Comparison.Greater
        };

        DepthStencilState = new DepthStencilState(MirrorServices.DirectXData.Device, dsDesc);
    }
    
    public override void Dispose()
    {
        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        
        DrawGBuffersHook?.Dispose();
        
        OMDrawIndexedHook?.Dispose();
        OMDrawHook?.Dispose();
        OMDrawIndexedInstancedHook?.Dispose();
        OMDrawInstancedHook?.Dispose();
        OMDrawAutoHook?.Dispose();
        OMDrawIndexedInstancedIndirectHook?.Dispose();
        OMDrawInstancedIndirectHook?.Dispose();
        OMFinishCommandListHook?.Dispose();
        
        PushbackUIHook?.Disable();
        PushbackUIHook?.Dispose();
        
        OmSetRenderTargetsHook?.Disable();
        OmSetRenderTargetsHook?.Dispose();
        
        OMPresentHook?.Disable();
        OMPresentHook?.Dispose();
        
        CubeModel?.Dispose();
        CameraBuffer?.Dispose();
        
        DepthStencilState?.Dispose();
        RasterizerState?.Dispose();
        
        TextureWrap?.Dispose();
        TextureResourceView?.Dispose();
    }
    
    public override void Init()
    { 
        DrawGBuffersHook.Enable();
        
        OMDrawIndexedHook?.Enable();
        OMDrawHook?.Enable();
        OMDrawIndexedInstancedHook?.Enable();
        OMDrawInstancedHook?.Enable();
        OMDrawAutoHook?.Enable();
        OMDrawIndexedInstancedIndirectHook?.Enable();
        OMDrawInstancedIndirectHook?.Enable();
        OMFinishCommandListHook?.Enable();
        
        OmSetRenderTargetsHook?.Enable();
        OMPresentHook?.Enable();
        PushbackUIHook?.Enable();
    }
    
    bool firstThisFrame = false;

    int countThisFrame = 0;
    
    private nint DrawGBuffersDetour(nint a1, nint a2)
    {
        MirrorServices.MirrorLog.LogVerbose("DrawGBuffersDetour");
        
        nint returner = DrawGBuffersHook!.OriginalDisposeSafe(a1, a2);
        
        MirrorServices.MirrorLog.LogVerbose("DrawGBuffersDetour END");
        
        return returner;
    }
    
    private int OMFinishCommandListDetour(nint context, byte restoreContext, nint* ppCommandList)
    {
        MirrorServices.MirrorLog.LogVerbose("ON FINNISH COMMAND LIST");
        
        return OMFinishCommandListHook!.OriginalDisposeSafe(context, restoreContext, ppCommandList);
    }
    
    private void OMDrawIndexedDetour(nint context, uint indexCount, uint startIndexLocation, int baseVertexLocation)
    {
        if (allowCount) OMDrawIndexedHookCount++;
        
        //uint min = Math.Min(indexCount, 200);
        
        OMDrawIndexedHook!.OriginalDisposeSafe(context, indexCount, startIndexLocation, baseVertexLocation);
    }

    private void OMDrawDetour(nint context, uint vertexCount, uint startVertexLocation)
    {
        if (allowCount) OMDrawHookCount++;
        
        OMDrawHook!.OriginalDisposeSafe(context, vertexCount, startVertexLocation);
    }

    private void OMDrawIndexedInstancedDetour(nint context, uint indexCountPerInstance, uint instanceCount, uint startIndexLocation, int baseVertexLocation, uint startInstanceLocation)
    {
        if (allowCount) OMDrawIndexedInstancedHookCount++;
        
        OMDrawIndexedInstancedHook!.OriginalDisposeSafe(context, indexCountPerInstance, instanceCount, startIndexLocation, baseVertexLocation, startInstanceLocation);
    }

    private void OMDrawInstancedDetour(nint context, uint vertexCountPerInstance, uint instanceCount, uint startVertexLocation, uint startInstanceLocation)
    {
        if (allowCount) OMDrawInstancedHookCount++;
        
        OMDrawInstancedHook!.OriginalDisposeSafe(context, vertexCountPerInstance, instanceCount, startVertexLocation, startInstanceLocation);
    }

    private void OMDrawAutoDetour(nint context)
    {
        if (allowCount) OMDrawAutoHookCount++;
        
        OMDrawAutoHook!.OriginalDisposeSafe(context);
    }

    private void OMDrawIndexedInstancedIndirectDetour(nint context, nint bufferForArgs, uint alignedByteOffsetForArgs)
    {
        if (allowCount) OMDrawIndexedInstancedIndirectHookCount++;
        
        OMDrawIndexedInstancedIndirectHook!.OriginalDisposeSafe(context, bufferForArgs, alignedByteOffsetForArgs);
    }

    private void OMDrawInstancedIndirectDetour(nint context, nint bufferForArgs, uint alignedByteOffsetForArgs)
    {
        if (allowCount) OMDrawInstancedIndirectHookCount++;
        
        OMDrawInstancedIndirectHook!.OriginalDisposeSafe(context, bufferForArgs, alignedByteOffsetForArgs);
    }
    
    private nint PushbackUIDetour(nint a1, char a2)
    {
        //MirrorServices.MirrorLog.LogInfo("PUSHBACK UI");
        
        return PushbackUIHook!.OriginalDisposeSafe(a1, a2);
    }
    
    private void OnScreenSizeChanged(uint newWidth, uint newHeight)
    {
        ScreenWidth  = newWidth;
        ScreenHeight = newHeight;
        
        BeforeUITexture?.Dispose();
        BeforeUITexture = null;
        
        CleanupOld();
        
        MirrorServices.MirrorLog.LogInfo($"Screen size changed: [{ScreenWidth}x{ScreenHeight}].");
    }
    
    private void SetRenderTargetFor(nint renderTargetView, nint depthStencilView)
    {
        if (!RenderTargetViews.ContainsKey(renderTargetView))
        {
            RenderTargetView rtv = new RenderTargetView(renderTargetView);
            
            RenderTargetViews.Add(renderTargetView, rtv);
        }
        
        PairCount.TryAdd((renderTargetView, depthStencilView), 0);
        
        PairCount[(renderTargetView, depthStencilView)]++;
    }
    
    int toggleAmount = 0;
    bool lastState = false;
    
    private void OMSetRenderTargetsDetour(nint device, uint numViews, nint* renderTargetViews, nint depthStencilView)
    {
        OmSetRenderTargetsHook!.Original(device, numViews, renderTargetViews, depthStencilView);
        
        if (numViews == 0)
        {
            return;
        }
        
        
        if (depthStencilView == nint.Zero)
        {
            return;
        }
        
        if (numViews != 5)
        {
            return;
        }
        
        allowCount = false;
        
        for (int i = 0; i < numViews; i++)
        {
            nint rtv = *(renderTargetViews + i);
            
            if (rtv != nint.Zero)
            {
                RenderTargetView rtvm = new RenderTargetView(rtv);
            
                Texture2D? texture2D = rtvm.Resource.QueryInterfaceOrNull<Texture2D>();
            
                if (texture2D != null)
                {
                    
                    
                    //allowCount |= ((nint)RenderTargetManager.Instance()->GBuffers[0].Value->D3D11Texture2D == texture2D.NativePointer);
                    //allowCount |= ((nint)RenderTargetManager.Instance()->GBuffers[1].Value->D3D11Texture2D == texture2D.NativePointer);
                    //allowCount |= ((nint)RenderTargetManager.Instance()->GBuffers[2].Value->D3D11Texture2D == texture2D.NativePointer);
                    //allowCount |= ((nint)RenderTargetManager.Instance()->GBuffers[3].Value->D3D11Texture2D == texture2D.NativePointer);
                    allowCount |= ((nint)RenderTargetManager.Instance()->GBuffers[4].Value->D3D11Texture2D == texture2D.NativePointer);
                    

                }
            }
            
            if (allowCount )
            {
                MirrorServices.MirrorLog.LogVerbose(numViews);
                
                CubeModel.BindBuffer();
                
                CubeModel.Draw();
                        
            }
            
            if (!uniqueRtv.TryAdd(rtv, 1))
            {
                uniqueRtv[rtv]++;
                
                continue;
            }

           
            
            TryGetTexture2DDescFromView(rtv, depthStencilView);
        }
        
        
    }
    
    private int OMPresentDetour(nint swapChain, uint syncInterval, uint flags)
    {
        presentCount++;
        
        _ = uniqueRtv.TryGetValue(lookupRTV, out ulong count);
        
        firstThisFrame = false;
        countThisFrame = 0;
        
        MirrorServices.MirrorLog.LogVerbose($"{OMDrawIndexedHook.Address}, {OMDrawIndexedHook.BackendName}");
        
        MirrorServices.MirrorLog.LogVerbose("TOGGLE AMOUNT: " + toggleAmount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawIndexedHookCount: " + OMDrawIndexedHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawHookCount: " + OMDrawHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawIndexedInstancedHookCount: " + OMDrawIndexedInstancedHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawInstancedHookCount: " + OMDrawInstancedHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawAutoHookCount: " + OMDrawAutoHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawIndexedInstancedIndirectHookCount: " + OMDrawIndexedInstancedIndirectHookCount);
        MirrorServices.MirrorLog.LogVerbose("OMDrawInstancedIndirectHookCount: " + OMDrawInstancedIndirectHookCount);
        
        toggleAmount = 0;
        bufferBound = 0;
        //MirrorServices.MirrorLog.LogVerbose($"OMPresentDetour: [{presentCount}] [{count}] [{uniqueRtvDsv.Count}] [{isSame}] [{uniqueRtv.Count}] [{MappedTextures.Count}].");
        
        foreach (MappedTexture mappedTexture in MappedTextures)
        {
            HandleMappedTexture(mappedTexture);
        }
        
        int returner = OMPresentHook!.Original(swapChain, syncInterval, flags);
        
        isSame = false;
        sameCounter = 0;
        uniqueRtv.Clear();
            
        foreach (MappedTexture mappedTexture in MappedTextures)
        {
            mappedTexture.Dispose();
        }
        
        MappedTextures.Clear();
        
        foreach (RenderTexture rTex in RenderTextures)
        {
            rTex.Dispose();
        }
        
        RenderTextures.Clear();
        
        
        OMDrawIndexedHookCount = 0;
        OMDrawHookCount = 0;
        OMDrawIndexedInstancedHookCount = 0;
        OMDrawInstancedHookCount = 0;
        OMDrawAutoHookCount = 0;
        OMDrawIndexedInstancedIndirectHookCount = 0;
        OMDrawInstancedIndirectHookCount = 0;

        
        return returner;
    }
    
    private void CleanupOld()
    {
        MirrorServices.MirrorLog.LogVerbose("RTV DISPOSE");
        
        PairCount.Clear();
        
        foreach (var rtv in RenderTargetViews.Values)
        {
            rtv?.Dispose();
        }
        
        RenderTargetViews.Clear();
    }
    
    private bool isSame = false;
    private int sameCounter = 0;
    
    private bool IsSame(nint texture, Texture* tex)
    { 
        if (tex == null)
            return false;
        return texture == (nint)tex->D3D11Texture2D;
    }
    
    public readonly List<RenderTexture> RenderTextures = [];
    
    private void HandleForColour(MappedTexture mappedTexture, Vector4 colour)
    {
        RenderTexture rTex = mappedTexture.CreateRenderTarget(MirrorServices.DirectXData);
        RenderTextures.Add(rTex);
        
        //MirrorServices.StatePreserver.PreserveState();
        
        ShaderHandler.ChannelMappedShader.Bind(mappedTexture, colour, rTex);
        
        ShaderHandler.ChannelMappedShader.Draw();
        
        ShaderHandler.ChannelMappedShader.UnbindTexture();
    }
    
    private void HandleMappedTexture(MappedTexture mappedTexture)
    {
        HandleForColour(mappedTexture, new Vector4(1.0f, 0.0f, 0.0f, 0.0f));
        HandleForColour(mappedTexture, new Vector4(0.0f, 1.0f, 0.0f, 0.0f));
        HandleForColour(mappedTexture, new Vector4(0.0f, 0.0f, 1.0f, 0.0f));
        HandleForColour(mappedTexture, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
    }
    
    
    private int bufferBound = 0;
    
    private unsafe bool TryGetTexture2DDescFromView(nint renderTargetView, nint depthStencilView)
    {
        if (renderTargetView == nint.Zero)
        {
            return false;
        }
        
        if (depthStencilView == nint.Zero)
        {
            return false;
        }
        
        MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();
        
        if (renderTargetManager == null)
        {
            return false;
        }
      

        
        try
        {
            DepthStencilView dsv = new DepthStencilView(depthStencilView);
            RenderTargetView rtv = new RenderTargetView(renderTargetView);
            
            Texture2D? texture2D = rtv.Resource.QueryInterfaceOrNull<Texture2D>();
            
            if (texture2D == null)
            {
                return false;
            }
            
            //allowCount = ((nint)RenderTargetManager.Instance()->GBuffers[0].Value->D3D11Texture2D == texture2D.NativePointer);
            
            /*
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUI)) MirrorServices.MirrorLog.LogVerbose("Back Buffer No UI");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy2)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy2");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy4)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy4");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy5)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy5");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy6)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy6");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy7)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy7");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy8)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy8");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy9)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy9");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy10)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy10");
            if (IsSame(texture2D.NativePointer, renderTargetManager->BackBufferNoUICopy11)) MirrorServices.MirrorLog.LogVerbose("BackBufferNoUICopy11");
            */
            
            if (texture2D.Description.BindFlags.HasFlag(BindFlags.ShaderResource))
            {
                MappedTexture mappedTexture = new MappedTexture(MirrorServices.DirectXData, ref texture2D, true);
                
                MappedTextures.Add(mappedTexture);
            }
            
            Texture2D? dsvTexture2D = dsv.Resource.QueryInterfaceOrNull<Texture2D>();
            
            if (dsvTexture2D == null)
            {
                return false;
            }
            
            /*
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparency)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparency");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy2)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy2");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy3)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy3");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy4)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy4");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy5)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy5");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy6)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy6");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy7)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy7");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy9)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy9");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy10)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy10");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy11)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy11");
            if (IsSame(dsvTexture2D.NativePointer, renderTargetManager->DepthBufferNoTransparencyCopy12)) MirrorServices.MirrorLog.LogVerbose("DepthBufferNoTransparencyCopy12");
            */
            
            if ((nint)RenderTargetManager.Instance()->GBuffers[0].Value->D3D11Texture2D == texture2D.NativePointer)
            {
                bufferBound++;
                
                lookupRTV = renderTargetView;
                
                if (!uniqueRtvDsv.TryAdd(depthStencilView, 1))
                {
                    uniqueRtvDsv[depthStencilView]++;
                }
                

                
                if ((nint)renderTargetManager->DepthBufferNoTransparency->D3D11Texture2D == dsvTexture2D.NativePointer)
                {
                    isSame = true;
                    sameCounter++;
                    
                    //RenderCube();
                }
                
                MirrorServices.MirrorLog.LogVerbose(texture2D.NativePointer + $" [{MappedTextures.Count}].");
            }
            
            
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
        
        return true;
    }
    
    private void RenderCube()
    {
        MirrorServices.StatePreserver.PreserveState();
       
        // TODO: RENDER CUBE HERE!
        
        ShaderHandler.ShadedModelShader.Bind();

        CubeModel.BindBuffer();

        // BIND TEXTURE
        MirrorServices.DirectXData.Context.PixelShader.SetShaderResource(0, TextureResourceView);
        // END BIND
        
        // BIND MATRIX
        CameraBufferLayout cameraMatrix = CameraHook.GetCameraBufferLayout(Matrix.Identity);

        CameraBuffer.UpdateConstantBuffer(ref cameraMatrix);

        CameraBuffer.BindToVertexShader(0);
        // END BIND
        
        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
        
        MirrorServices.DirectXData.Context.OutputMerger.SetBlendState(new BlendState(MirrorServices.DirectXData.Device, blendDesc));

        MirrorServices.DirectXData.Context.Rasterizer.State = RasterizerState;

        MirrorServices.DirectXData.Context.OutputMerger.DepthStencilState = DepthStencilState;

        CubeModel.Draw();
        
        ShaderHandler.ShadedModelShader.Release();

        MirrorServices. DirectXData.Context.Rasterizer.State = null;
        MirrorServices.DirectXData.Context.OutputMerger.DepthStencilState = null;
        MirrorServices.DirectXData.Context.OutputMerger.SetBlendState(null);
        
        MirrorServices.StatePreserver.RestoreState();
    }
}


public class MirrorsRenderTargetView
{
    public ulong Calls
        { get; private set; } = 0;
    
    public ulong LastPresented
        { get; private set; } = 0;
    
    public RenderTargetView RenderTargetView
        { get; }
    
    public MirrorsRenderTargetView(RenderTargetView renderTargetView)
    {
        RenderTargetView = renderTargetView;
    }
    
    public void RegisterCall()
    {
        Calls++;
    }
    
    public void PresentedAt(ulong presentIndex)
    {
        LastPresented = presentIndex;
    }
}

/*
[0]	6ED3F979	(CMTUseCountedObject<CDXGISwapChain>::QueryInterface)
[1]	6ED3F84D	(CMTUseCountedObject<CDXGISwapChain>::AddRef)
[2]	6ED3F77D	(CMTUseCountedObject<CDXGISwapChain>::Release)
[3]	6ED6A6D7	(CDXGISwapChain::SetPrivateData)
[4]	6ED6A904	(CDXGISwapChain::SetPrivateDataInterface)
[5]	6ED72BC9	(CDXGISwapChain::GetPrivateData)
[6]	6ED6DCDD	(CDXGISwapChain::GetParent)
[7]	6ED69BF4	(CDXGISwapChain::GetDevice)
[8]	6ED3FAAD	(CDXGISwapChain::Present)
[9]	6ED40209	(CDXGISwapChain::GetBuffer)
[10]	6ED47C1C	(CDXGISwapChain::SetFullscreenState)
[11]	6ED48CD9	(CDXGISwapChain::GetFullscreenState)
[12]	6ED40CB1	(CDXGISwapChain::GetDesc)
[13]	6ED48A3B	(CDXGISwapChain::ResizeBuffers)
[14]	6ED6F153	(CDXGISwapChain::ResizeTarget)
[15]	6ED47BA5	(CDXGISwapChain::GetContainingOutput)
[16]	6ED6D9B5	(CDXGISwapChain::GetFrameStatistics)
[17]	6ED327B5	(CDXGISwapChain::GetLastPresentCount)
[18]	6ED43400	(CDXGISwapChain::GetDesc1)
[19]	6ED6D9D0	(CDXGISwapChain::GetFullscreenDesc)
[20]	6ED6DA90	(CDXGISwapChain::GetHwnd)
[21]	6ED6D79F	(CDXGISwapChain::GetCoreWindow)
[22]	6ED6E352	(?Present1@?QIDXGISwapChain2@@CDXGISwapChain@@UAGJIIPBUDXGI_PRESENT_PARAMETERS@@@Z)
[23]	6ED6E240	(CDXGISwapChain::IsTemporaryMonoSupported)
[24]	6ED44146	(CDXGISwapChain::GetRestrictToOutput)
[25]	6ED6F766	(CDXGISwapChain::SetBackgroundColor)
[26]	6ED6D6B9	(CDXGISwapChain::GetBackgroundColor)
[27]	6ED4417B	(CDXGISwapChain::SetRotation)
[28]	6ED6DDE3	(CDXGISwapChain::GetRotation)
[29]	6ED6FF85	(CDXGISwapChain::SetSourceSize)
[30]	6ED6DF4F	(CDXGISwapChain::GetSourceSize)
[31]	6ED6FCBD	(CDXGISwapChain::SetMaximumFrameLatency)
[32]	6ED6DBE5	(CDXGISwapChain::GetMaximumFrameLatency)
[33]	6ED6D8CD	(CDXGISwapChain::GetFrameLatencyWaitableObject)
[34]	6ED6FB45	(CDXGISwapChain::SetMatrixTransform)
[35]	6ED6DAD0	(CDXGISwapChain::GetMatrixTransform)
[36]	6ED6C155	(CDXGISwapChain::CheckMultiplaneOverlaySupportInternal)
[37]	6ED6E82D	(CDXGISwapChain::PresentMultiplaneOverlayInternal)
[38]	6ED4397A	(CMTUseCountedObject<CDXGISwapChain>::`vector deleting destructor')
[39]	6ED4EAE0	(CSwapBuffer::AddRef)
[40]	6ED46C81	(CMTUseCountedObject<CDXGISwapChain>::LUCBeginLayerDestruction)
 */

/*
[0]	6C5F62A6	(CContext::ID3D11DeviceContext2_QueryInterface_Thk)
[1]	6C5F628C	(CContext::ID3D11DeviceContext2_AddRef_Thk)
[2]	6C5F1B7D	(CContext::ID3D11DeviceContext2_Release_Thk)
[3]	6C64F652	(CContext::ID3D11DeviceContext2_GetDevice_)
[4]	6C64F67F	(CContext::ID3D11DeviceContext2_GetPrivateData_)
[5]	6C64F6D9	(CContext::ID3D11DeviceContext2_SetPrivateData_)
[6]	6C64F6B9	(CContext::ID3D11DeviceContext2_SetPrivateDataInterface_)
[7]	6C5F2BBC	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,0>)
[8]	6C5F22C0	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,4>)
[9]	6C5F3265	(CContext::ID3D11DeviceContext2_SetShader_<1,4>)
[10]	6C5F32F6	(CContext::ID3D11DeviceContext2_SetSamplers_<1,4>)
[11]	6C5F33C5	(CContext::ID3D11DeviceContext2_SetShader_<1,0>)
[12]	6C5F2D28	(CContext::ID3D11DeviceContext2_DrawIndexed_<1>)
[13]	6C5F3677	(CContext::ID3D11DeviceContext2_Draw_<1>)
[14]	6C5F1A57	(CContext::ID3D11DeviceContext2_Map_<1>)
[15]	6C5F1A79	(CContext::ID3D11DeviceContext2_Unmap_<1>)
[16]	6C5F2892	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,4>)
[17]	6C5F3456	(CContext::ID3D11DeviceContext2_IASetInputLayout_<1>)
[18]	6C5F1FFB	(CContext::ID3D11DeviceContext2_IASetVertexBuffers_<1>)
[19]	6C5F1F72	(CContext::ID3D11DeviceContext2_IASetIndexBuffer_<1>)
[20]	6C5F6A08	(CContext::ID3D11DeviceContext2_DrawIndexedInstanced_<1>)
[21]	6C61938A	(CContext::ID3D11DeviceContext2_DrawInstanced_<1>)
[22]	6C632B40	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,3>)
[23]	6C5F3DC2	(CContext::ID3D11DeviceContext2_SetShader_<1,3>)
[24]	6C5F2399	(CContext::ID3D11DeviceContext2_IASetPrimitiveTopology_<1>)
[25]	6C5F3CF5	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,0>)
[26]	6C5F35DC	(CContext::ID3D11DeviceContext2_SetSamplers_<1,0>)
[27]	6C60C4E9	(CContext::ID3D11DeviceContext2_Begin_<1>)
[28]	6C5F6A35	(CContext::ID3D11DeviceContext2_End_<1>)
[29]	6C5F735D	(CContext::ID3D11DeviceContext2_GetData_<1>)
[30]	6C5F37EE	(CContext::ID3D11DeviceContext2_SetPredication_<1>)
[31]	6C5F3D1E	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,3>)
[32]	6C5F35F8	(CContext::ID3D11DeviceContext2_SetSamplers_<1,3>)
[33]	6C5F23E8	(CContext::ID3D11DeviceContext2_OMSetRenderTargets_<1>)
[34]	6C5F538E	(CContext::ID3D11DeviceContext2_OMSetRenderTargetsAndUnorderedAccessViews_<1>)
[35]	6C5F34BC	(CContext::ID3D11DeviceContext2_OMSetBlendState_<1>)
[36]	6C5F3568	(CContext::ID3D11DeviceContext2_OMSetDepthStencilState_<1>)
[37]	6C5F41A9	(CContext::ID3D11DeviceContext2_SOSetTargets_<1>)
[38]	6C618838	(CContext::ID3D11DeviceContext2_DrawAuto_<1>)
[39]	6C6188EA	(CContext::ID3D11DeviceContext2_DrawIndexedInstancedIndirect_<1>)
[40]	6C618F86	(CContext::ID3D11DeviceContext2_DrawInstancedIndirect_<1>)
[41]	6C61850F	(CContext::ID3D11DeviceContext2_Dispatch_<1>)
[42]	6C6181C7	(CContext::ID3D11DeviceContext2_DispatchIndirect_<1>)
[43]	6C5F1B12	(CContext::ID3D11DeviceContext2_RSSetState_<1>)
[44]	6C5F1CE5	(CContext::ID3D11DeviceContext2_RSSetViewports_<1>)
[45]	6C5F277A	(CContext::ID3D11DeviceContext2_RSSetScissorRects_<1>)
[46]	6C5F6A60	(CContext::ID3D11DeviceContext2_CopySubresourceRegion_<1>)
[47]	6C612046	(CContext::ID3D11DeviceContext2_CopyResource_<1>)
[48]	6C5F1B97	(CContext::ID3D11DeviceContext2_UpdateSubresource_<1>)
[49]	6C612341	(CContext::ID3D11DeviceContext2_CopyStructureCount_<1>)
[50]	6C5F5945	(CContext::ID3D11DeviceContext2_ClearRenderTargetView_<1>)
[51]	6C610F81	(CContext::ID3D11DeviceContext2_ClearUnorderedAccessViewUint_<1>)
[52]	6C610A5C	(CContext::ID3D11DeviceContext2_ClearUnorderedAccessViewFloat_<1>)
[53]	6C5FA896	(CContext::ID3D11DeviceContext2_ClearDepthStencilView_<1>)
[54]	6C61D8F4	(CContext::ID3D11DeviceContext2_GenerateMips_<1>)
[55]	6C63507F	(CContext::ID3D11DeviceContext2_SetResourceMinLOD_<1>)
[56]	6C61E1AD	(CContext::ID3D11DeviceContext2_GetResourceMinLOD_<1>)
[57]	6C62A863	(CContext::ID3D11DeviceContext2_ResolveSubresource_<1>)
[58]	6C6198ED	(CContext::ID3D11DeviceContext2_ExecuteCommandList_<1>)
[59]	6C5F3D47	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,1>)
[60]	6C5F3E3D	(CContext::ID3D11DeviceContext2_SetShader_<1,1>)
[61]	6C5F3614	(CContext::ID3D11DeviceContext2_SetSamplers_<1,1>)
[62]	6C63153B	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,1>)
[63]	6C5F3D70	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,2>)
[64]	6C5F3EB8	(CContext::ID3D11DeviceContext2_SetShader_<1,2>)
[65]	6C5F3635	(CContext::ID3D11DeviceContext2_SetSamplers_<1,2>)
[66]	6C6316B8	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,2>)
[67]	6C5F3D99	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,5>)
[68]	6C5F3FB0	(CContext::ID3D11DeviceContext2_CSSetUnorderedAccessViews_<1>)
[69]	6C5F3F33	(CContext::ID3D11DeviceContext2_SetShader_<1,5>)
[70]	6C5F3656	(CContext::ID3D11DeviceContext2_SetSamplers_<1,5>)
[71]	6C631835	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,5>)
[72]	6C645CC3	(CContext::ID3D11DeviceContext2_VSGetConstantBuffers_<1>)
[73]	6C627412	(CContext::ID3D11DeviceContext2_PSGetShaderResources_<1>)
[74]	6C6275ED	(CContext::ID3D11DeviceContext2_PSGetShader_<1>)
[75]	6C627125	(CContext::ID3D11DeviceContext2_PSGetSamplers_<1>)
[76]	6C646318	(CContext::ID3D11DeviceContext2_VSGetShader_<1>)
[77]	6C627033	(CContext::ID3D11DeviceContext2_PSGetConstantBuffers_<1>)
[78]	6C61EF84	(CContext::ID3D11DeviceContext2_IAGetInputLayout_<1>)
[79]	6C61F09C	(CContext::ID3D11DeviceContext2_IAGetVertexBuffers_<1>)
[80]	6C61EE2C	(CContext::ID3D11DeviceContext2_IAGetIndexBuffer_<1>)
[81]	6C61D2AF	(CContext::ID3D11DeviceContext2_GSGetConstantBuffers_<1>)
[82]	6C61D869	(CContext::ID3D11DeviceContext2_GSGetShader_<1>)
[83]	6C61F080	(CContext::ID3D11DeviceContext2_IAGetPrimitiveTopology_<1>)
[84]	6C64613D	(CContext::ID3D11DeviceContext2_VSGetShaderResources_<1>)
[85]	6C645FA2	(CContext::ID3D11DeviceContext2_VSGetSamplers_<1>)
[86]	6C5FD919	(CContext::ID3D11DeviceContext2_GetPredication_<1>)
[87]	6C61D68E	(CContext::ID3D11DeviceContext2_GSGetShaderResources_<1>)
[88]	6C61D3A1	(CContext::ID3D11DeviceContext2_GSGetSamplers_<1>)
[89]	6C5F670E	(CContext::ID3D11DeviceContext2_OMGetRenderTargets_<1>)
[90]	6C62088C	(CContext::ID3D11DeviceContext2_OMGetRenderTargetsAndUnorderedAccessViews_<1>)
[91]	6C62039C	(CContext::ID3D11DeviceContext2_OMGetBlendState_<1>)
[92]	6C62053E	(CContext::ID3D11DeviceContext2_OMGetDepthStencilState_<1>)
[93]	6C62B170	(CContext::ID3D11DeviceContext2_SOGetTargets_<1>)
[94]	6C62798D	(CContext::ID3D11DeviceContext2_RSGetState_<1>)
[95]	6C5FA41D	(CContext::ID3D11DeviceContext2_RSGetViewports_<1>)
[96]	6C627806	(CContext::ID3D11DeviceContext2_RSGetScissorRects_<1>)
[97]	6C61E950	(CContext::ID3D11DeviceContext2_HSGetShaderResources_<1>)
[98]	6C61EC61	(CContext::ID3D11DeviceContext2_HSGetShader_<1>)
[99]	6C61E8EB	(CContext::ID3D11DeviceContext2_HSGetSamplers_<1>)
[100]	6C61E60C	(CContext::ID3D11DeviceContext2_HSGetConstantBuffers_<1>)
[101]	6C616E8A	(CContext::ID3D11DeviceContext2_DSGetShaderResources_<1>)
[102]	6C617065	(CContext::ID3D11DeviceContext2_DSGetShader_<1>)
[103]	6C616CEF	(CContext::ID3D11DeviceContext2_DSGetSamplers_<1>)
[104]	6C616A10	(CContext::ID3D11DeviceContext2_DSGetConstantBuffers_<1>)
[105]	6C60CD5A	(CContext::ID3D11DeviceContext2_CSGetShaderResources_<1>)
[106]	6C60D2BE	(CContext::ID3D11DeviceContext2_CSGetUnorderedAccessViews_<1>)
[107]	6C60CFA9	(CContext::ID3D11DeviceContext2_CSGetShader_<1>)
[108]	6C60CCF5	(CContext::ID3D11DeviceContext2_CSGetSamplers_<1>)
[109]	6C60CB4C	(CContext::ID3D11DeviceContext2_CSGetConstantBuffers_<1>)
[110]	6C5FA518	(CContext::ID3D11DeviceContext2_ClearState_<1>)
[111]	6C5F7A84	(CContext::ID3D11DeviceContext2_Flush_AppEntered)
[112]	6C64F6A2	(CContext::ID3D11DeviceContext2_GetType_)
[113]	6C61DC03	(CContext::ID3D11DeviceContext2_GetContextFlags_<1>)
[114]	6C61CAB1	(CContext::ID3D11DeviceContext2_FinishCommandList_<1>)
[115]	6C5F2ED9	(CContext::ID3D11DeviceContext2_CopySubresourceRegion1_<1>)
[116]	6C5F6E28	(CContext::ID3D11DeviceContext2_UpdateSubresource1_<1>)
[117]	6C5F5ED6	(CContext::ID3D11DeviceContext2_DiscardResource_<1>)
[118]	6C5F7055	(CContext::ID3D11DeviceContext2_DiscardView_<1>)
[119]	6C5F3A43	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,0>)
[120]	6C5F3C55	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,1>)
[121]	6C5F3C7D	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,2>)
[122]	6C5F3CA5	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,3>)
[123]	6C5F3830	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,4>)
[124]	6C5F3CCD	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,5>)
[125]	6C645BF7	(CContext::ID3D11DeviceContext2_VSGetConstantBuffers1_<1>)
[126]	6C61E540	(CContext::ID3D11DeviceContext2_HSGetConstantBuffers1_<1>)
[127]	6C616944	(CContext::ID3D11DeviceContext2_DSGetConstantBuffers1_<1>)
[128]	6C61D148	(CContext::ID3D11DeviceContext2_GSGetConstantBuffers1_<1>)
[129]	6C626DB1	(CContext::ID3D11DeviceContext2_PSGetConstantBuffers1_<1>)
[130]	6C60C714	(CContext::ID3D11DeviceContext2_CSGetConstantBuffers1_<1>)
[131]	6C5F1214	(CContext::ID3D11DeviceContext2_SwapDeviceContextState_<1>)
[132]	6C5F6B1B	(CContext::ID3D11DeviceContext2_ClearView_<1>)
[133]	6C5F2DDC	(CContext::ID3D11DeviceContext2_DiscardView1_<1>)
[134]	6C64130C	(CContext::ID3D11DeviceContext2_UpdateTileMappings_<1>)
[135]	6C612D6B	(CContext::ID3D11DeviceContext2_CopyTileMappings_<1>)
[136]	6C615CBD	(CContext::ID3D11DeviceContext2_CopyTiles_<1>)
[137]	6C645368	(CContext::ID3D11DeviceContext2_UpdateTiles_<1>)
[138]	6C628E89	(CContext::ID3D11DeviceContext2_ResizeTilePool_<1>)
[139]	6C63F155	(CContext::ID3D11DeviceContext2_TiledResourceBarrier_<1>)
[140]	6C62004F	(CContext::ID3D11DeviceContext2_IsAnnotationEnabled_<1>)
[141]	6C634C76	(CContext::ID3D11DeviceContext2_SetMarkerInt_<1>)
[142]	6C60C3E1	(CContext::ID3D11DeviceContext2_BeginEventInt_<1>)
[143]	6C619693	(CContext::ID3D11DeviceContext2_EndEvent_<1>)
*/