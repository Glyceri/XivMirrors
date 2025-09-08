using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.MirrorsEdge.Services;
using System.Collections.Generic;

namespace MirrorsEdge.MirrorsEdge.Hooking.HookableElements;

internal unsafe class TextureHooker : HookableElement
{
    private delegate Texture* Device_CreateTexture2DDelegate(Device* device, int* size, byte mipLevel, TextureFormat format, uint flags, int unk);

    [Signature("E8 ?? ?? ?? ?? 48 89 86 30 09 00 00", DetourName = nameof(Device_CreateTexture2DDetour))]
    private readonly Hook<Device_CreateTexture2DDelegate>? Device_CreateTexture2DHook = null;

    public readonly List<nint> Textures = new List<nint>();

    public TextureHooker(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {

    }

    public override void Init()
    {
        //Device_CreateTexture2DHook?.Enable();
    }

    private Texture* Device_CreateTexture2DDetour(Device* device, int* size, byte mipLevel, TextureFormat format, uint flags, int unk)
    {
        MirrorServices.MirrorLog.Log("Created texture 2D");

        Texture* texture = Device_CreateTexture2DHook!.OriginalDisposeSafe(device, size, mipLevel, format, flags, unk);

        if (texture != null)
        {
            Textures.Add((nint)texture);
        }

        return texture;
    }

    public override void OnDispose()
    {
        Device_CreateTexture2DHook?.Dispose();
    }
}
