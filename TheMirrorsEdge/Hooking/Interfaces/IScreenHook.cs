using TheMirrorsEdge.Hooking.Elements;

namespace TheMirrorsEdge.Hooking.Interfaces;

public interface IScreenHook : IHookableElement
{
    void RegisterScreenSizeChangeCallback(ScreenHook.ScreensizeDelegate onScreenSizeChange);
    void DeregisterScreenSizeChangeCallback(ScreenHook.ScreensizeDelegate onScreenSizeChange);
    
    /// <summary>
    /// Special spaghetti method hooked right before IMGUI draw.
    /// </summary>
    void OnImGuiDraw();
}