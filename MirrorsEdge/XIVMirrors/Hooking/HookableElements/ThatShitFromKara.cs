using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.WhateverTheFlipFlop;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

// https://github.com/karashiiro/Simulacrum/blob/main/src/simulacrum-dalamud-plugin/Game/Primitive.cs#L103
internal unsafe class ThatShitFromKara : HookableElement
{
    private delegate nint CreateApricotTextureFromTex(nint apricot, nint texturePointer, long textureLength);
    private delegate nint PrimitiveServerCtorDelegate(nint thisPtr);
    private delegate byte PrimitiveServerInitialize(nint thisPtr, int unk1, int unk2, int unk3, int unk4, int unk5, nint unk6, nint deviceContext, nint unknownPrimitiveInitSettings);
    private delegate void PrimitiveServerLoadResource(nint thisPtr);
    private delegate void PrimitiveServerBegin(nint thisPtr);
    private delegate void PrimitiveServerSpursSortUnencumbered(nint thisPtr);
    private delegate void PrimitiveServerRender(nint thisPtr);
    public delegate nint PrimitiveContextDrawCommand(nint thisPtr, ulong unk1, uint unk2, uint unk3, nint unk4);
    private delegate nint KernelDeviceCreateVertexDeclaration(nint thisPtr, nint unk1, uint unk2);
    private delegate nint EnvironmentManagerUpdate(nint thisPtr, nint unk1);

    [Signature("E8 ?? ?? ?? ?? 48 89 47 10 48 8B C8", DetourName = nameof(PrimitiveServerCtorDetour))]
    private readonly Hook<PrimitiveServerCtorDelegate>? PrimitiveServerCtorHook = null!;

    [Signature("E8 ?? ?? ?? ?? 84 C0 74 2B 48 85 F6", DetourName = nameof(PrimitiveServerInitializeDetour))]
    private readonly Hook<PrimitiveServerInitialize>? PrimitiveServerInitializeHook = null!;

    [Signature("E8 ?? ?? ?? ?? 84 C0 74 12 48 8B 4B 10", DetourName = nameof(PrimitiveServerLoadResourceDetour))]
    private readonly Hook<PrimitiveServerLoadResource>? PrimitiveServerLoadResourceHook = null!;

    [Signature("48 89 5C 24 08 57 48 83 EC 20 33 FF 48 8B D9 48 89 B9 90 00 00 00", DetourName = nameof(PrimitiveServerBeginDetour))]
    private readonly Hook<PrimitiveServerBegin>? PrimitiveServerBeginHook = null!;

    [Signature("40 53 48 83 EC 20 48 8B D9 48 8B 49 30 E8 ?? ?? ?? ?? 48 8D 54 24 ??", DetourName = nameof(PrimitiveServerSpursSortUnencumberedDetour))]
    private readonly Hook<PrimitiveServerSpursSortUnencumbered>? PrimitiveServerSpursSortUnencumberedHook = null!;

    [Signature("48 89 5C 24 10 48 89 74 24 18 57 48 81 EC A0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 90 00 00 00 65 48 8B 04 25 58 00 00 00", DetourName = nameof(PrimitiveServerRenderDetour))]
    private readonly Hook<PrimitiveServerRender>? PrimitiveServerRenderHook = null!;

    [Signature("E8 ?? ?? ?? ?? 4C 8B C0 48 85 C0 0F 84 ?? ?? ?? ?? F3 0F 10 4B 04", DetourName = nameof(PrimitiveContextDrawCommandDetour))]
    private readonly Hook<PrimitiveContextDrawCommand>? PrimitiveContextDrawCommandHook = null!;

    [Signature("E8 ?? ?? ?? ?? 48 8B 4E 28 48 89 04 F9", DetourName = nameof(KernelDeviceCreateVertexDeclarationDetour))]
    private readonly Hook<KernelDeviceCreateVertexDeclaration>? KernelDeviceCreateVertexDeclarationHook = null!;

    [Signature("48 89 5C 24 ?? 55 56 57 41 55 41 56 48 8D AC 24 ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? 48 8B 05", DetourName = nameof(EnvironmentManagerUpdateDetour))]
    private readonly Hook<EnvironmentManagerUpdate>? EnvironmentManagerUpdateHook = null!;

    [Signature("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 40 48 8B FA 41 8B F0", DetourName = nameof(CreateApricotTextureFromTexDetour))]
    private readonly Hook<CreateApricotTextureFromTex>? CreateApricotTextureFromTexHook = null!;

    private readonly Primitive primitive;

    private readonly CancellationTokenSource source;

    private readonly WhateverTheFlipFlop.Material material;

    private readonly DirectXData Data;

    public ThatShitFromKara(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData data) : base(dalamudServices, mirrorServices)
    {
        Data = data;

        InitDepthState();

        primitive = new Primitive(DalamudServices, mirrorServices, this);

        source = new CancellationTokenSource();

        MyRenderTargetManager* mrtm = (MyRenderTargetManager*)RenderTargetManager.Instance();

        material = WhateverTheFlipFlop.Material.CreateFromTexture((nint)mrtm->BackBufferNoUI, data);

        DalamudServices.DalamudPlugin.UiBuilder.Draw += Draw;
    }

    private void Draw()
    {

    }

    public override void Init()
    {
        CreateApricotTextureFromTexHook?.Enable();
        PrimitiveServerCtorHook?.Enable();
        PrimitiveServerInitializeHook?.Enable();
        PrimitiveServerLoadResourceHook?.Enable();
        PrimitiveServerBeginHook?.Enable();
        PrimitiveServerSpursSortUnencumberedHook?.Enable();
        PrimitiveServerRenderHook?.Enable();
        PrimitiveContextDrawCommandHook?.Enable();
        KernelDeviceCreateVertexDeclarationHook?.Enable();
        EnvironmentManagerUpdateHook?.Enable();

        primitive.Initialize();
    }

    public nint CreateApricotTextureFromTexDetour(nint apricot, nint texturePointer, long textureLength)
    {
        MirrorServices.MirrorLog.Log("Create Apricot Texture");

        return CreateApricotTextureFromTexHook!.Original(apricot, texturePointer, textureLength);
    }

    private nint PrimitiveServerCtorDetour(nint thisPtr)
    {
        MirrorServices.MirrorLog.LogVerbose("Primative Ctor");

        return PrimitiveServerCtorHook!.Original(thisPtr);
    }

    private byte PrimitiveServerInitializeDetour(nint thisPtr, int unk1, int unk2, int unk3, int unk4, int unk5, nint unk6, nint unk7, nint unk8)
    {
        MirrorServices.MirrorLog.LogVerbose("PrimitiveServerInitializeDetour");

        return PrimitiveServerInitializeHook!.Original(thisPtr, unk1, unk2, unk3, unk4, unk5, unk6, unk7, unk8);
    }

    private void PrimitiveServerLoadResourceDetour(nint thisPtr)
    {
        MirrorServices.MirrorLog.LogVerbose("PrimitiveServerLoadResourceDetour");

        PrimitiveServerLoadResourceHook!.Original(thisPtr);
    }

    private void PrimitiveServerBeginDetour(nint thisPtr)
    {
        MirrorServices.MirrorLog.LogVerbose("PrimitiveServerBeginDetour");

        PrimitiveServerBeginHook!.Original(thisPtr);
    }

    private void PrimitiveServerSpursSortUnencumberedDetour(nint thisPtr)
    {
        MirrorServices.MirrorLog.LogVerbose("PrimitiveServerSpursSortUnencumberedDetour");

        PrimitiveServerSpursSortUnencumberedHook!.Original(thisPtr);
    }

    private void PrimitiveServerRenderDetour(nint thisPtr)
    {
        MirrorServices.MirrorLog.LogVerbose("PrimitiveServerRenderDetour");

        PrimitiveServerRenderHook!.Original(thisPtr);
    }

    private nint PrimitiveContextDrawCommandDetour(nint primitiveContext, ulong unk1, uint unk2, uint unk3, nint unk4)
    {
        //MirrorServices.MirrorLog.LogVerbose("PrimitiveContextDrawCommandDetour");

        return PrimitiveContextDrawCommandHook!.Original(primitiveContext, unk1, unk2, unk3, unk4);
    }

    private nint KernelDeviceCreateVertexDeclarationDetour(nint thisPtr, nint unk1, uint unk2)
    {
        MirrorServices.MirrorLog.LogVerbose("KernelDeviceCreateVertexDeclarationDetour");

        return KernelDeviceCreateVertexDeclarationHook!.Original(thisPtr, unk1, unk2);
    }

    private static DepthStencilState? worldDepthState;

    public void InitDepthState()
    {
        var desc = new DepthStencilStateDescription
        {
            IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.LessEqual,
            IsStencilEnabled = false
        };

        worldDepthState = new DepthStencilState(Data.Device, desc);
    }

    private nint EnvironmentManagerUpdateDetour(nint thisPtr, nint unk1)
    {
        MirrorServices.MirrorLog.LogVerbose($"EnvironmentManagerUpdateDetour: {thisPtr}, {unk1}");


        nint outcome = EnvironmentManagerUpdateHook!.Original(thisPtr, unk1);
        
        try
        {
            PrimitiveServerBeginDetour(primitive.PrimitiveServer);

            EnvMangDetour();
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        try
        {
            PrimitiveServerSpursSortUnencumberedDetour(primitive.PrimitiveServer);

            PrimitiveServerRenderDetour(primitive.PrimitiveServer);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        return outcome;
    }

    [StructLayout(LayoutKind.Sequential, Size = 12)]
    public struct Position
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3 ToVector3()
        {
            return new Vector3
            {
                X = X,
                Y = Y,
                Z = Z,
            };
        }

        public static Position FromCoordinates(float x, float y, float z)
        {
            return new Position
            {
                X = x,
                Y = y,
                Z = z,
            };
        }

        public static Position FromVector3(Vector3 vector)
        {
            return new Position
            {
                X = vector.X,
                Y = vector.Y,
                Z = vector.Z,
            };
        }

        public static implicit operator Position(Vector3 vector) => FromVector3(vector);

        public static implicit operator Vector3(Position position) => position.ToVector3();
    }

    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static Color FromRGBA(byte r, byte g, byte b, byte a)
        {
            return new Color
            {
                R = r,
                G = g,
                B = b,
                A = a,
            };
        }

        public static implicit operator Color(Vector4 color)
        {
            var r = (byte)Math.Floor(color.X * 255);
            var g = (byte)Math.Floor(color.Y * 255);
            var b = (byte)Math.Floor(color.Z * 255);
            var a = (byte)Math.Floor(color.W * 255);
            return FromRGBA(r, g, b, a);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public Position Position;
        public Color Color;
        public UV UV;
    }

    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public struct UV
    {
        public float U;
        public float V;

        public static UV FromUV(float u, float v)
        {
            return new UV
            {
                U = u,
                V = v,
            };
        }
    }

    public class Location
    {
        public int Territory { get; init; }

        public int World { get; init; }

        public Position Position { get; init; }
    }

    private void EnvMangDetour()
    {
        if (DalamudServices.ClientState.LocalPlayer == null)
        {
            return;
        }

        var context = new UnmanagedPrimitiveContext(primitive.PrimitiveContext, PrimitiveContextDrawCommandDetour);

          //Data.Context.OutputMerger.SetDepthStencilState(worldDepthState);

        var vertexPointer = context.DrawCommand(0x23, 4, 5, material.Pointer);

        if (vertexPointer == nint.Zero)
        {
            MirrorServices.MirrorLog.LogVerbose("Received empty vertex pointer");

            return;
        }

        var aspectRatio = (float)1080 / 1920;
        var dimensions = new Vector3(1, aspectRatio, 0);
        var translation = new Vector3(0, 0, 0);
        var scale = 10;
        var color = new Vector4(1, 1, 1, 1);
        var position = Position.FromCoordinates(DalamudServices.ClientState.LocalPlayer.Position.X, DalamudServices.ClientState.LocalPlayer.Position.Y, DalamudServices.ClientState.LocalPlayer.Position.Z);

        //var position = Position.FromCoordinates(1, 1, 1);

        unsafe
        {
            _ = new Span<Vertex>((void*)vertexPointer, 4)
            {
                [0] = new Vertex
                {
                    Position = position + translation + Vector3.UnitY * dimensions * scale,
                    Color = color,
                    UV = UV.FromUV(0, 0),
                },
                [1] = new Vertex
                {
                    Position = position + translation,
                    Color = color,
                    UV = UV.FromUV(0, 1),
                },
                [2] = new Vertex
                {
                    Position = position + translation + dimensions * scale,
                    Color = color,
                    UV = UV.FromUV(1, 0),
                },
                [3] = new Vertex
                {
                    Position = position + translation + Vector3.UnitX * dimensions * scale,
                    Color = color,
                    UV = UV.FromUV(1, 1),
                },
            };
        }
    }

    public nint CreateVertexDeclaration(FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device* device, nint unk1, uint unk2)
    {
        return KernelDeviceCreateVertexDeclarationDetour((nint)device, unk1, unk2);
    }

    public nint Construct(nint thisPtr)
    {
        return PrimitiveServerCtorDetour(thisPtr);
    }

    public byte InitializePrimitiveServer(nint thisPtr, int unk1, int unk2, int unk3, int unk4, int unk5, nint unk6, nint unk7, nint unk8)
    {
        return PrimitiveServerInitializeDetour(thisPtr, unk1, unk2, unk3, unk4, unk5, unk6, unk7, unk8);
    }

    public void LoadResource(nint thisPtr)
    {
        PrimitiveServerLoadResourceDetour(thisPtr);
    }

    protected override void OnDispose()
    {
        DalamudServices.DalamudPlugin.UiBuilder.Draw -= Draw;

        material?.Dispose();

        source?.Cancel();
        source?.Dispose();

        primitive?.Dispose();

        CreateApricotTextureFromTexHook?.Dispose();
        PrimitiveServerCtorHook?.Dispose();
        PrimitiveServerInitializeHook?.Dispose();
        PrimitiveServerLoadResourceHook?.Dispose();
        PrimitiveServerBeginHook?.Dispose();
        PrimitiveServerSpursSortUnencumberedHook?.Dispose();
        PrimitiveServerRenderHook?.Dispose();
        PrimitiveContextDrawCommandHook?.Dispose();
        KernelDeviceCreateVertexDeclarationHook?.Dispose();
        EnvironmentManagerUpdateHook?.Dispose();
    }
}
