using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct MyRenderTargetManager
{
    /// <summary>
    /// The depth buffer without any transparent elements buffered.
    /// </summary>
    [FieldOffset(0x70)]  public Texture* DepthBufferNoTransparency;

    /// <summary>
    /// The depth buffer with transparent elements on top.
    /// </summary>
    [FieldOffset(0x90)]  public Texture* DepthBufferTransparency;

    /// <summary>
    /// The final back buffer with NO ui.
    /// </summary>
    [FieldOffset(0x258)] public Texture* BackBufferNoUI;

    /// <summary>
    /// The final back buffer shown on screen.
    /// </summary>
    [FieldOffset(0x4E0)] public Texture* BackBuffer;    
}
