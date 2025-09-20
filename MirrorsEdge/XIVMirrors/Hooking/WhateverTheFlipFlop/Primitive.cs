using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements;
using MirrorsEdge.XIVMirrors.Hooking.Structs;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Runtime.InteropServices;

namespace MirrorsEdge.XIVMirrors.Hooking.WhateverTheFlipFlop;

internal unsafe class Primitive : IDisposable
{
    private readonly DalamudServices    DalamudServices;
    private readonly MirrorServices     MirrorServices;
    private readonly ThatShitFromKara   ThatShitFromKara;

    private const int VertexDeclarationBufferElements = 3;

    private readonly nint _vertexDeclarationBuffer;

    public nint PrimitiveServer;
    public nint PrimitiveContext; 

    public Primitive(DalamudServices dalamudServices, MirrorServices mirrorServices, ThatShitFromKara thatShitFromKara) 
    { 
        DalamudServices  = dalamudServices;
        MirrorServices   = mirrorServices;
        ThatShitFromKara = thatShitFromKara;

        _vertexDeclarationBuffer = Marshal.AllocHGlobal(VertexDeclarationBufferElements * Marshal.SizeOf<InputElement>()); 

        PrimitiveServer = Marshal.AllocHGlobal(200);
    }

    public void Initialize()
    {
        Span<InputElement> vertexDeclarationElements = new Span<InputElement>((InputElement*)_vertexDeclarationBuffer, VertexDeclarationBufferElements);
        
        SetVertexDeclarationOptions(vertexDeclarationElements);

        MirrorServices.MirrorLog.LogVerbose("Create Vertex Declaration");
        nint vertexDeclaration = ThatShitFromKara.CreateVertexDeclaration(FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance(), _vertexDeclarationBuffer, VertexDeclarationBufferElements);

        MirrorServices.MirrorLog.LogVerbose("Construct PrimitiveServer");
        ThatShitFromKara.Construct(PrimitiveServer);

        byte[] initializeSettings = new byte[24];

        fixed (byte* ptr = initializeSettings)
        {
            Marshal.WriteInt64((nint)ptr, 0x00000000_000A0000);
            Marshal.WriteInt64((nint)ptr + 8, 0x00000000_00280000);
            Marshal.WriteInt64((nint)ptr + 16, 0x00000000_000A0000);

            MirrorServices.MirrorLog.LogVerbose("InitializePrimitiveServer");
            _ = ThatShitFromKara.InitializePrimitiveServer(PrimitiveServer, 0x01, 0x1E, 0x0C, 0x0F, 0, 24, vertexDeclaration, (nint)ptr);
        }

        MirrorServices.MirrorLog.LogVerbose("LoadResource");
        ThatShitFromKara.LoadResource(PrimitiveServer);

        MirrorServices.MirrorLog.LogVerbose("Set some wank ass pointer");
        PrimitiveContext = Marshal.ReadIntPtr(PrimitiveServer + 0xB8);

        MirrorServices.MirrorLog.LogInfo("Constructed Primitive Context");
    }

    private void SetVertexDeclarationOptions(Span<InputElement> elements)
    {
        if (elements.Length != VertexDeclarationBufferElements)
        {
            throw new InvalidOperationException("Buffer size mismatch.");
        }

        elements[0] = new InputElement
        {
            Slot = 0,
            Offset = 0x00,
            Format = 0x13,
            Semantic = 0x00,
        };

        elements[1] = new InputElement
        {
            Slot = 0,
            Offset = 0x0C,
            Format = 0x24,
            Semantic = 0x03,
        };

        elements[2] = new InputElement
        {
            Slot = 0,
            Offset = 0x10,
            Format = 0x12,
            Semantic = 0x08,
        };
    }

    public void Dispose()
    {
        Marshal.FreeHGlobal(PrimitiveServer);
        Marshal.FreeHGlobal(_vertexDeclarationBuffer);

        GC.SuppressFinalize(this);
    }
}
