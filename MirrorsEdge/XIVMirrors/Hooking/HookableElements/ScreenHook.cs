using Dalamud.Hooking;
using Dalamud.Interface.Utility;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class ScreenHook : HookableElement
{
    public  delegate void ScreensizeDelegate(uint newWidth, uint newHeight);
    private delegate int  OMResizeBuffersDelegate(nint swapChain, uint bufferCount, uint width, uint height, Format format, uint swapChainFlags);

    private readonly List<ScreensizeDelegate>       screenSizeListeners = [];
    private readonly Hook<OMResizeBuffersDelegate>? omResizeBuffersHook;

    private uint lastWidth  = 0;
    private uint lastHeight = 0;

    public ScreenHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData) : base(dalamudServices, mirrorServices)
    {
        nint swapChainVTable                = GetVTable(directXData.SwapChain.NativePointer);

        nint vtableResizeBuffersAddress     = GetVTableAddress(swapChainVTable, 13);

        omResizeBuffersHook                 = DalamudServices.Hooking.HookFromAddress<OMResizeBuffersDelegate>(vtableResizeBuffersAddress, OMResizeBuffersDetour);
    }

    public override void Init()
    {
        omResizeBuffersHook?.Enable();

        HandleImGuiScreenSize();
    }
    
    private void HandleImGuiScreenSize()
    {
        Vector2 currentWorkSize = ImGuiHelpers.MainViewport.WorkSize;

        uint currentWidth       = (uint)currentWorkSize.X;
        uint currentHeight      = (uint)currentWorkSize.Y;

        HandleObtainedSize(currentWidth, currentHeight);
    }

    private void HandleObtainedSize(uint currentWidth, uint currentHeight)
    {
        if (currentWidth == lastWidth && currentHeight == lastHeight)
        {
            return;
        }

        try
        {
            RunCallbacks(currentWidth, currentHeight);
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }

        lastWidth = currentWidth;
        lastHeight = currentHeight;
    }

    private void RunCallbacks(uint newWidth, uint newHeight)
    {
        MirrorServices.MirrorLog.LogInfo($"XivMirrors detected a change in screensize [{newWidth}, {newHeight}].");

        foreach (ScreensizeDelegate screenSizeDelegate in screenSizeListeners)
        {
            try
            {
                screenSizeDelegate?.Invoke(newWidth, newHeight);
            }
            catch (Exception ex)
            {
                MirrorServices.MirrorLog.LogError(ex, "An error occured when relaying screen size changed.");
            }
        }
    }

    private int OMResizeBuffersDetour(nint swapChain, uint bufferCount, uint width, uint height, Format format, uint swapChainFlags)
    {
        MirrorServices.MirrorLog.LogVerbose($"Buffer Size Change Requested by the game [{bufferCount}, {width}, {height}, {format}, {swapChainFlags}].");

        HandleObtainedSize(width, height);

        return omResizeBuffersHook!.Original(swapChain, bufferCount, width, height, format, swapChainFlags);
    }

    public void RegisterScreenSizeChangeCallback(ScreensizeDelegate onScreenSizeChange)
    {
        _ = screenSizeListeners.Remove(onScreenSizeChange);

        screenSizeListeners.Add(onScreenSizeChange);
    }

    public void DeregisterScreenSizeChangeCallback(ScreensizeDelegate onScreenSizeChange)
    {
        _ = screenSizeListeners.Remove(onScreenSizeChange);
    }

    /// <summary>
    /// Special spaghetti method hooked right before IMGUI draw.
    /// </summary>
    public void OnImGuiDraw()
    {
        HandleImGuiScreenSize();
    }

    protected override void OnDispose()
    {
        omResizeBuffersHook?.Disable();
        omResizeBuffersHook?.Dispose();

        screenSizeListeners.Clear();
    }
}
