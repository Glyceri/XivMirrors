using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using KernalDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    private readonly DirectXData DirectXData;

    public delegate void RenderPassDelegate(RenderPass renderPass);
    

    private delegate void DXGIPresentDelegate(IntPtr ptr);
    private delegate char SomethingDelegate(RenderTargetManager* renderTargetManager, int* size);


    private delegate void RenderThreadSetRenderTargetDelegate(KernalDevice* deviceInstance, SetRenderTargetCommand* command);

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? F3 0F 10 5F 18", DetourName = nameof(RenderThreadSetRenderTargetDetour))]
    private Hook<RenderThreadSetRenderTargetDelegate>? RenderThreadSetRenderTargetHook = null;

    [Signature("E8 ?? ?? ?? ?? C6 46 79 00 48 8B 8E 88 0A 0E 00", DetourName = nameof(DXGIPresentDetour))]
    private readonly Hook<DXGIPresentDelegate>? DXGIPresentHook = null;



    //[Signature("48 8B 3D ?? ?? ?? ?? 0F 29 70 D8", ScanType = ScanType.StaticAddress, Offset = 2)]
    public readonly SomeManagerStruct* SomeManagerInstance = null!;

    // sub_1402C3620
    [Signature("E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 8B 4F 18")]
    private readonly Hook<SomethingDelegate>? SomethingHook = null;

    private readonly List<RenderPassDelegate> _renderPasses         = [];

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData) : base(dalamudServices, mirrorServices)
    {
        nint address = DalamudServices.SigScanner.ScanText("48 8B 3D ?? ?? ?? ?? 0F 29 70 D8");

        address += *(int*)(address + 3) + 7;

        SomeManagerInstance = (SomeManagerStruct*)address;

        DirectXData = directXData;
    }

    public override void Init()
    {
        DXGIPresentHook?.Enable();
        
        //RenderThreadSetRenderTargetHook?.Enable();
    }

    private void DXGIPresentDetour(IntPtr ptr)
    {
        try
        {
            foreach (RenderPassDelegate renderPass in _renderPasses)
            {
                renderPass?.Invoke(RenderPass.Pre);
            }

            DXGIPresentHook!.Original(ptr);

            foreach (RenderPassDelegate renderPass in _renderPasses)
            {
                renderPass?.Invoke(RenderPass.Post);
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }
    }

    private readonly List<nint> UniqueDepthBuffers = new List<nint>();
    public readonly List<ShaderResourceView> SRVs = new List<ShaderResourceView>();

    private void RenderThreadSetRenderTargetDetour(KernalDevice* deviceInstance, SetRenderTargetCommand* command)
    {
        try
        {
            RenderThreadSetRenderTargetHook!.Original(deviceInstance, command);

            if (command->DepthBuffer != null)
            {
                nint dBuffer = (nint)command->DepthBuffer;


                bool removed = UniqueDepthBuffers.Remove(dBuffer);
                UniqueDepthBuffers.Add(dBuffer);

                if (!removed)
                {
                    SRVs.Add(new ShaderResourceView((nint)command->DepthBuffer->D3D11ShaderResourceView));
                }
            }
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, "erm");
        }
    }

    public void RegisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = _renderPasses.Remove(renderDelegate);

        _renderPasses.Add(renderDelegate);
    }

    public void DeregisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = _renderPasses.Remove(renderDelegate);
    }

    public override void OnDispose()
    {
        _renderPasses.Clear();

        RenderThreadSetRenderTargetHook?.Disable();
        RenderThreadSetRenderTargetHook?.Dispose();

        DXGIPresentHook?.Disable();
        DXGIPresentHook?.Dispose();
    }
}
