using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct MyDevice
{
    /// <summary>
    /// Some Texture
    /// <para>Size: 4 x 4</para>
    /// <para>Format: B8G8R8A8_UNORM</para>
    /// <para>Flags: Managed</para>
    /// 
    /// Actually these transform into the texture you screenshot later on
    /// </summary>
    [FieldOffset(0x920)] public Texture* SomeDummyTexture;

    /// <summary>
    /// Some Texture
    /// <para>Size: 4 x 4</para>
    /// <para>Format: B8G8R8A8_UNORM</para>
    /// <para>Flags: Managed</para>
    /// 
    /// Actually these transform into the texture you screenshot later on
    /// </summary>
    [FieldOffset(0x928)] public Texture* SomeDummyTexture1;

    /// <summary>
    /// Some Texture
    /// <para>Size: 4 x 4</para>
    /// <para>Format: B8G8R8A8_UNORM</para>
    /// <para>Flags: Managed</para>
    /// 
    /// Actually these transform into the texture you screenshot later on
    /// </summary>
    [FieldOffset(0x930)] public Texture* SomeDummyTexture2;
}

