using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using MirrorsEdge.MirrorsEdge.Hooking;
using MirrorsEdge.MirrorsEdge.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace MirrorsEdge.mirrorsedge.Hooking.HookableElements;

internal class ScreenHook : HookableElement
{
    private Vector2 lastSize = Vector2.Zero;

    private readonly List<Action<Vector2>> screenSizeListeners = new List<Action<Vector2>>();

    public ScreenHook(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
    }

    public override void Init()
    {
        DalamudServices.Framework.Update += OnScreenHookUpdate;
    }

    private void OnScreenHookUpdate(IFramework framework)
    {
        Vector2 size = ImGuiHelpers.MainViewport.Size;

        if (lastSize != size)
        {
            lastSize = size;

            OnSizeChange();
        }
    }

    private void OnSizeChange()
    {
        MirrorServices.MirrorLog.LogInfo($"Mirrors just detected that your screen size changed. It is now: {lastSize}. This WILL impact your current mirrors.");

        foreach (Action<Vector2> screenSizeCallback in screenSizeListeners)
        {
            try
            {
                screenSizeCallback?.Invoke(lastSize);
            }
            catch (Exception e)
            {
                MirrorServices.MirrorLog.LogException(e);
            }
        }
    }

    public void RegisterScreenSizeChangeCallback(Action<Vector2> onScreenSizeChange)
    {
        _ = screenSizeListeners.Remove(onScreenSizeChange);

        screenSizeListeners.Add(onScreenSizeChange);
    }

    public void DeregisterScreenSizeChangeCallback(Action<Vector2> onScreenSizeChange)
    {
        _ = screenSizeListeners.Remove(onScreenSizeChange);
    }

    public Vector2 GetCurrentScreenSize()
    {
        return lastSize;
    }

    public void SetupSize(ref Vector2 size)
    {
        size = lastSize;
    }

    public override void OnDispose()
    {
        DalamudServices.Framework.Update -= OnScreenHookUpdate;

        screenSizeListeners.Clear();
    }
}
