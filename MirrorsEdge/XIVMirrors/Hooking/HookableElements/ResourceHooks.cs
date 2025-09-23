using Dalamud.Hooking;
using Dalamud.Utility;
using Dalamud.Utility.Signatures;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.ResourceHandling;
using MirrorsEdge.XIVMirrors.Services;
using Penumbra.String.Classes;
using System;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

// Ive found some important paths:

// bg/ffxiv/fst_f1/twn/f1ti/bgplate/0000.mdl                -> Literally a flat plane
// bg/ffxiv/fst_f1/twn/common/texture/f1t0_i00_flor1_d.tex  -> Floor texture, 1024x1024 (1 texture, no coordinates used W)
// bg/ffxiv/fst_f1/twn/common/material/f1t0_i00_flor1.mtrl  -> The material of that floor. (it has a normal map and specular map, but you can replace them with VVV)

// dummy_d.tex  -> dummy texture for colour
// dummy_n.tex  -> dummy texture for normal
// dummy_s.tex  -> dummy texture for specular

internal unsafe class ResourceHooks : HookableElement
{
    private const uint MAX_PATH_SIZE = 260;

    private readonly ResourceHandler ResourceHandler;

    private delegate byte   LoadTexFileLocalDelegate(TextureResourceHandle* handle, int unk1, SeFileDescriptor* unk2, bool unk33);
    private delegate byte   TexResourceHandleOnLoadPrototype(TextureResourceHandle* handle, SeFileDescriptor* descriptor, byte unk2);
    private delegate IntPtr CheckFileStatePrototypeDelegate(IntPtr unk1, ulong crc64);
    private delegate byte   MaterialResourceHandle_LoadTexFilesDelegate(FFXIVClientStructs.FFXIV.Client.System.Resource.Handle.MaterialResourceHandle* handle);
    private delegate byte   ReadFilePrototypeDelegate(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync);
    private delegate byte   ReadSqpackPrototypeDelegate(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync);
    private delegate void*  GetResourceSyncPrototypeDelegate(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams);
    private delegate void*  GetResourceAsyncPrototypeDelegate(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown);

    [Signature("E8 ?? ?? ?? ?? 84 C0 B9 ?? ?? ?? ?? BA ?? ?? ?? ?? 0F 44 CA 0F B6 C1 48 83 C4 ?? C3 0F B6 C2", DetourName = nameof(MaterialResourceHandle_LoadTexFilesDetour))]
    private readonly Hook<MaterialResourceHandle_LoadTexFilesDelegate>? MaterialResourceHandle_LoadTexFilesHook = null!;

    [Signature("E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81", DetourName = nameof(GetResourceSyncPrototypeDetour))]
    private readonly Hook<GetResourceSyncPrototypeDelegate>? GetResourceSyncHook = null!;

    [Signature("E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00", DetourName = nameof(GetResourceAsyncPrototypeDetour))]
    private readonly Hook<GetResourceAsyncPrototypeDelegate>? GetResourceAsyncHook = null!;

    [Signature("40 56 41 56 48 83 EC ?? 0F BE 02", DetourName = nameof(ReadSqpackPrototypeDetour))]
    private readonly Hook<ReadSqpackPrototypeDelegate>? ReadSqpackHook = null!;

    [Signature("E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B C8", DetourName = nameof(CheckFileStateDetour))]
    private readonly Hook<CheckFileStatePrototypeDelegate>? CheckFileStatePrototypeHook = null!;

    [Signature("40 53 55 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B D9", DetourName = nameof(TexOnLoadDetour))]
    private readonly Hook<TexResourceHandleOnLoadPrototype>? TextureOnLoadHook = null!;

    private readonly ReadFilePrototypeDelegate  ReadFile;
    private readonly LoadTexFileLocalDelegate   LoadTexFileLocal;

    private bool TextureReturned = false;

    [Signature("48 8B 05 ?? ?? ?? ?? B3")]
    private readonly nint LodConfig = nint.Zero;

    public ResourceHooks(DalamudServices dalamudServices, MirrorServices mirrorServices, ResourceHandler resourceHandler) : base(dalamudServices, mirrorServices)
    {
        ResourceHandler = resourceHandler;

        nint readFileSignatureBinding = DalamudServices.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 63 42");


        ReadFile = Marshal.GetDelegateForFunctionPointer<ReadFilePrototypeDelegate>(readFileSignatureBinding);

        nint loadTexFileAddress = DalamudServices.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC ?? 49 8B E8 44 88 4C 24");

        LoadTexFileLocal = Marshal.GetDelegateForFunctionPointer<LoadTexFileLocalDelegate>(loadTexFileAddress);
    }

    public override void Init()
    {
        //TextureOnLoadHook?.Enable();

        //MaterialResourceHandle_LoadTexFilesHook?.Enable();
        //GetResourceSyncHook?.Enable();
        //GetResourceAsyncHook?.Enable();
        //ReadSqpackHook?.Enable();

        //CheckFileStatePrototypeHook?.Enable();
    }

    private byte GetLod(TextureResourceHandle* handle)
    {
        if (handle->ChangeLod)
        {
            int config = *(byte*)LodConfig + 0xE;

            if (config == byte.MaxValue)
            {
                return 2;
            }
        }

        return 0;
    }

    private byte TexOnLoadDetour(TextureResourceHandle* handle, SeFileDescriptor* descriptor, byte unk2)
    {
        byte ret = TextureOnLoadHook!.Original(handle, descriptor, unk2);

        if (!TextureReturned)
        {
            return ret;
        }

        // Function failed on a replaced texture, call local.
        TextureReturned = false;

        return LoadTexFileLocal(handle, GetLod(handle), descriptor, unk2 != 0);
    }

    private byte MaterialResourceHandle_LoadTexFilesDetour(FFXIVClientStructs.FFXIV.Client.System.Resource.Handle.MaterialResourceHandle* handle)
    {
        MirrorServices.MirrorLog.LogVerbose("Just loaded a .tex file c:");

        return MaterialResourceHandle_LoadTexFilesHook!.Original(handle);
    }

    private byte ReadSqpackPrototypeDetour(IntPtr fileHandler, SeFileDescriptor* fileDesc, int priority, bool isSync)
    {
        MirrorServices.MirrorLog.LogVerbose("Read SQPack Prototype");

        return ReadSqpackHook!.Original(fileHandler, fileDesc, priority, isSync);
    }

    private void* GetResourceOriginalCallback(SyncStatus syncStatus, IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown)
    {
        if (syncStatus == SyncStatus.Synchronized)
        {
            return GetResourceSyncHook!.Original(resourceManager, categoryId, resourceType, resourceHash, path, resParams);
        }
        else
        {
            return GetResourceAsyncHook!.Original(resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
        }
    }

    private void* GetResourceSyncPrototypeDetour(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams)
    {
        return GetResourceHandler(SyncStatus.Synchronized, resourceManager, categoryId, resourceType, resourceHash, path, resParams, false);
    }

    private void* GetResourceAsyncPrototypeDetour(IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown)
    {
        return GetResourceHandler(SyncStatus.NotSynchronized, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
    }

    private void* GetResourceHandler(SyncStatus syncStatus, IntPtr resourceManager, uint* categoryId, ResourceType* resourceType, int* resourceHash, byte* path, GetResourceParameters* resParams, bool isUnknown)
    {
        if (!Utf8GamePath.FromPointer(path, Penumbra.String.MetaDataComputation.None, out Utf8GamePath gamePath))
        {
            MirrorServices.MirrorLog.LogVerbose($"Unable to resolve path for loaded resource.");

            return GetResourceOriginalCallback(syncStatus, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
        }

        string filePath = gamePath.Path.ToString();

        MirrorServices.MirrorLog.LogVerbose($"Just loaded resource: [{filePath}].");

        string replacePath = filePath;


        // TODO: Implement proper replace path function here
        if (filePath == "bg/ffxiv/fst_f1/twn/common/texture/f1t0_i00_flor1_d.tex")
        {
            MirrorServices.MirrorLog.LogFatal("REPLACED!");

            replacePath = "D:\\FFXIV\\XIV MIRRORS\\dummy_d.tex\0";
        }

        if (filePath == "bg/ffxiv/fst_f1/twn/common/texture/f1t0_i00_flor1_n.tex")
        {
            MirrorServices.MirrorLog.LogFatal("REPLACED N!");

            replacePath = "D:\\FFXIV\\XIV MIRRORS\\dummy_n.tex\0";
        }

        if (filePath == "bg/ffxiv/fst_f1/twn/common/texture/f1t0_i00_flor1_s.tex")
        {
            MirrorServices.MirrorLog.LogFatal("REPLACED S!");

            replacePath = "D:\\FFXIV\\XIV MIRRORS\\dummy_s.tex\0";
        }

        if (replacePath == filePath)
        {
            MirrorServices.MirrorLog.LogVerbose("'replacePath' and 'filePath' are equal.");

            return GetResourceOriginalCallback(syncStatus, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
        }

        if (replacePath.IsNullOrWhitespace())
        {
            MirrorServices.MirrorLog.Log("'replacePath' is either null or whitespace.");

            return GetResourceOriginalCallback(syncStatus, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
        }

        if (replacePath.Length >= MAX_PATH_SIZE)
        {
            MirrorServices.MirrorLog.Log($"'replacePath.Length' [{replacePath.Length}] is bigger than the allowed maximum [{MAX_PATH_SIZE}].");

            return GetResourceOriginalCallback(syncStatus, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
        }

        FullPath resolvedPath = new FullPath(replacePath);

        ResourceHandler.AddCrc(*resourceType, resolvedPath);

        *resourceHash = MirrorServices.Utils.ComputeHash(resolvedPath.InternalName, resParams);

        path = resolvedPath.InternalName.Path;

        return GetResourceOriginalCallback(syncStatus, resourceManager, categoryId, resourceType, resourceHash, path, resParams, isUnknown);
    }

    private nint CheckFileStateDetour(nint ptr, ulong crc64)
    {
        if (ResourceHandler.HasCRC(crc64))
        {
            TextureReturned = true;
        }

        return CheckFileStatePrototypeHook!.Original(ptr, crc64);
    }

    protected override void OnDispose()
    {
        TextureOnLoadHook?.Dispose();
        CheckFileStatePrototypeHook?.Dispose();

        MaterialResourceHandle_LoadTexFilesHook?.Dispose();
        GetResourceSyncHook?.Dispose();
        GetResourceAsyncHook?.Dispose();
        ReadSqpackHook?.Dispose();
    }
}
