using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MirrorsEdge.XIVMirrors.Rendering;

[StructLayout(LayoutKind.Explicit)]
internal unsafe struct MyImmediateContext
{
    /// <summary>
    /// 0x20 ShaderCodeResourceHandle* "shader/sm5/shcd/SaveTextureRightsVS.shcd"
    /// 0x28 ShaderCodeResourceHandle* "shader/sm5/shcd/SaveTextureRightsPS.shcd"
    /// 0x30 TextureResourceHandle* "common/graphics/texture/-screenShotCopyRights.tex"
    /// </summary>
    [FieldOffset(0x728)] public void* ScreenshotShaderHolderThing;

    /// <summary>
    /// Shader for "shader/sm5/shcd/ClearVS.shcd"
    /// </summary>
    [FieldOffset(0x700)] public Shader* ClearVSShader;

    /// <summary>
    /// Shader for "shader/sm5/shcd/ClearPS.shcd"
    /// </summary>
    [FieldOffset(0x708)] public Shader* ClearPSShader;

    /// <summary>
    /// Shader for "shader/sm5/shcd/CopyVS.shcd"
    /// </summary>
    [FieldOffset(0x1808)] public Shader* CopyVSShader;

    /// <summary>
    /// Shader for "shader/sm5/shcd/CopyPS.shcd"
    /// </summary>
    [FieldOffset(0x1810)] public Shader* CopyPSShader;

    /// <summary>
    /// Shader for "shader/sm5/shcd/CopyValidColorPS.shcd"
    /// </summary>
    [FieldOffset(0x1818)] public Shader* CopyValidColorPSShader;

    /// <summary>
    /// Shader for "shader/sm5/shcd/DepthCopyVS.shcd"
    /// </summary>
    [FieldOffset(0x1820)] public Shader* DepthCopyVS;

    /// <summary>
    /// Shader for "shader/sm5/shcd/DepthCopyPS.shcd"
    /// </summary>
    [FieldOffset(0x1828)] public Shader* DepthCopyPS;

    /// <summary>
    /// "shader/sm5/shcd/ClearVS.shcd"
    /// </summary>
    [FieldOffset(0x6F0)] public ShaderCodeResourceHandle* ClearVS;

    /// <summary>
    /// "shader/sm5/shcd/ClearPS.shcd"
    /// </summary>
    [FieldOffset(0x6F8)] public ShaderCodeResourceHandle* ClearPS;

    /// <summary>
    /// "shader/sm5/shcd/CopyVS.shcd"
    /// </summary>
    [FieldOffset(0x17E0)] public ShaderCodeResourceHandle* CopyVS;

    /// <summary>
    /// "shader/sm5/shcd/CopyPS.shcd"
    /// </summary>
    [FieldOffset(0x17E8)] public ShaderCodeResourceHandle* CopyPS;

    /// <summary>
    /// "shader/sm5/shcd/CopyValidColorPS.shcd"
    /// </summary>
    [FieldOffset(0x17F0)] public ShaderCodeResourceHandle* CopyValidColorPS;

    /// <summary>
    /// "shader/sm5/shcd/DepthCopyVS.shcd"
    /// </summary>
    [FieldOffset(0x17F8)] public ShaderCodeResourceHandle* DepthCopyVS;


    /// <summary>
    /// "shader/sm5/shcd/DepthCopyPS.shcd"
    /// </summary>
    [FieldOffset(0x1800)] public ShaderCodeResourceHandle* DepthCopyPS;
}
