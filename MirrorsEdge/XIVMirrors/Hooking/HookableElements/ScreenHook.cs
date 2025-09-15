using Dalamud.Hooking;
using Dalamud.Interface.Utility;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class ScreenHook : HookableElement
{
    private readonly RendererHook RendererHook;

    public delegate void ScreensizeDelegate(int newWidth, int newHeight);

    private delegate bool GameWindow_SetWindowSizeDelegate(GameWindow* gameWindow, int newWidth, int newHeight);

    [Signature("E8 ?? ?? ?? ?? 42 0F B7 84 BF 90 00 00 00", DetourName = nameof(GameWindow_SetWindowSizeDetour))]
    private readonly Hook<GameWindow_SetWindowSizeDelegate>? GameWindow_SetWindowSizeHook = null;

    private readonly List<ScreensizeDelegate> screenSizeListeners = [];

    private Vector2 _lastWindowSize;

    public ScreenHook(DalamudServices dalamudServices, MirrorServices mirrorServices, RendererHook rendererHook) : base(dalamudServices, mirrorServices)
    {
        RendererHook = rendererHook;

        RendererHook.RegisterRenderPassListener(OnRenderPass);
    }

    public override void Init()
    {
        GameWindow_SetWindowSizeHook?.Enable();
    }
    
    private void HandleImGuiScreenSize()
    {
        if (_lastWindowSize == ImGuiHelpers.MainViewport.WorkSize)
        {
            return;
        }

        _lastWindowSize = ImGuiHelpers.MainViewport.WorkSize;

        RunCallbacks((int)_lastWindowSize.X, (int)_lastWindowSize.Y);

        MirrorServices.MirrorLog.LogVerbose(ImGuiHelpers.MainViewport.WorkSize);
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        HandleImGuiScreenSize();
    }

    private void RunCallbacks(int newWidth, int newHeight)
    {
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

    private bool GameWindow_SetWindowSizeDetour(GameWindow* gameWindow, int newWidth, int newHeight)
    {
        MirrorServices.MirrorLog.LogVerbose($"Detected a new window size: [{newWidth}, {newHeight}]");

        RunCallbacks(newWidth, newHeight);

        return GameWindow_SetWindowSizeHook!.Original(gameWindow, newWidth, newHeight);
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

    public void OnImGuiDraw()
    {
        HandleImGuiScreenSize();
    }

    public override void OnDispose()
    {
        RendererHook.DeregisterRenderPassListener(OnRenderPass);

        GameWindow_SetWindowSizeHook?.Disable();
        GameWindow_SetWindowSizeHook?.Dispose();

        screenSizeListeners.Clear();
    }
}
