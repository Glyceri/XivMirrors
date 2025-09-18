using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Services;
using Penumbra.String.Classes;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.XIVMirrors.ResourceHandling;

internal class ResourceHandler : IDisposable
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;

    private readonly HashSet<ulong>  CustomTexCrc       = [];

    public ResourceHandler(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
    }

    public void AddCrc(ResourceType resourceType, FullPath fullPath)
    {
        if (resourceType != ResourceType.Tex)
        {
            return;
        }

        ulong crc64 = fullPath.Crc64;

        _ = CustomTexCrc.Add(crc64);
    }

    public bool HasCRC(ulong crc64)
    {
        if (CustomTexCrc.Contains(crc64))
        {
            return true;
        }

        return false;
    }

    public void Dispose()
    {
        CustomTexCrc.Clear();
    }
}
