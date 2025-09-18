using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using Lumina.Data;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class ResourceHooks : HookableElement
{
    private delegate byte  MaterialResourceHandle_LoadTexFilesDelegate(MaterialResourceHandle* handle);
    private delegate byte  ReadFilePrototypeDelegate(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync);
    private delegate byte  ReadSqpackPrototypeDelegate(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync);
    private delegate void* GetResourceSyncPrototypeDelegate(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams);
    private delegate void* GetResourceAsyncPrototypeDelegate(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown);

    [Signature("E8 ?? ?? ?? ?? 84 C0 B9 ?? ?? ?? ?? BA ?? ?? ?? ?? 0F 44 CA 0F B6 C1 48 83 C4 ?? C3 0F B6 C2", DetourName = nameof(MaterialResourceHandle_LoadTexFilesDetour))]
    private readonly Hook<MaterialResourceHandle_LoadTexFilesDelegate>? MaterialResourceHandle_LoadTexFilesHook = null!;

    [Signature("E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81", DetourName = nameof(GetResourceSyncPrototypeDetour))]
    private readonly Hook<GetResourceSyncPrototypeDelegate>? GetResourceSyncHook = null!;

    [Signature("E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00", DetourName = nameof(GetResourceAsyncPrototypeDetour))]
    private readonly Hook<GetResourceAsyncPrototypeDelegate>? GetResourceAsyncHook = null!;

    [Signature("40 56 41 56 48 83 EC ?? 0F BE 02", DetourName = nameof(ReadSqpackPrototypeDetour))]
    private readonly Hook<ReadSqpackPrototypeDelegate>? ReadSqpackHook = null!;

    private readonly ReadFilePrototypeDelegate ReadFile;

    public ResourceHooks(DalamudServices dalamudServices, MirrorServices mirrorServices) : base(dalamudServices, mirrorServices)
    {
        nint readFileSignatureBinding = DalamudServices.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 63 42");

        ReadFile = Marshal.GetDelegateForFunctionPointer<ReadFilePrototypeDelegate>(readFileSignatureBinding);
    }

    public override void Init()
    {
        MaterialResourceHandle_LoadTexFilesHook?.Enable();
        GetResourceSyncHook?.Enable();
        GetResourceAsyncHook?.Enable();
        ReadSqpackHook?.Enable();
    }

    private byte MaterialResourceHandle_LoadTexFilesDetour(MaterialResourceHandle* handle)
    {
        MirrorServices.MirrorLog.LogVerbose("Just loaded a .tex file c:");

        return MaterialResourceHandle_LoadTexFilesHook!.Original(handle);
    }

    private byte ReadSqpackPrototypeDetour(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync)
    {
        MirrorServices.MirrorLog.LogVerbose("Read SQPack Prototype");

        return ReadSqpackHook!.Original(fileHandler, fileDesc, priority, isSync);
    }


    private void* GetResourceSyncPrototypeDetour(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams)
    {
        MirrorServices.MirrorLog.LogVerbose("GetResource[Sync]PrototypeDetour");

        return GetResourceSyncHook!.Original(resourceManager, categoryId, resourceType, resourceHash, path, resParams);
    }

    private void* GetResourceAsyncPrototypeDetour(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown)
    {
        MirrorServices.MirrorLog.LogVerbose("GetResource[Async]PrototypeDetour");

        return GetResourceAsyncHook!.Original(resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
    }

    public override void OnDispose()
    {
        MaterialResourceHandle_LoadTexFilesHook?.Dispose();
        GetResourceSyncHook?.Dispose();
        GetResourceAsyncHook?.Dispose();
        ReadSqpackHook?.Dispose();
    }
}
