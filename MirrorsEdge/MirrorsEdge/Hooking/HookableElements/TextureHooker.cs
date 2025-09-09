using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.MirrorsEdge.Services;
using Silk.NET.Direct3D11;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class TextureHooker : HookableElement
{
    private delegate Texture* Device_CreateTexture2DDelegate(Device* device, int* size, byte mipLevel, TextureFormat format, uint flags, int unk);

    //[Signature("E8 ?? ?? ?? ?? 48 89 86 30 09 00 00", DetourName = nameof(Device_CreateTexture2DDetour))]
    //private readonly Hook<Device_CreateTexture2DDelegate>? Device_CreateTexture2DHook = null;

    public TextureHooker(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
        
    }

    public override void Init()
    {
        //Device_CreateTexture2DHook?.Enable();
    }


    public override void OnDispose()
    {
        //Device_CreateTexture2DHook?.Dispose();
    }
}
