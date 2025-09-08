using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Interop;
using Lumina.Models.Models;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Memory;
using MirrorsEdge.MirrorsEdge.Services;
using Silk.NET.Direct3D11;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

unsafe class D3DBuffer(ID3D11Buffer* buffer, uint length) : IDisposable
{
    public ID3D11Buffer* Handle = buffer;
    public uint Length = length;

    public void Dispose()
    {
        Handle->Release();
    }
}

unsafe class VertexBuffer(Vertex[] vertices, D3DBuffer buffer) : IDisposable
{
    public Vertex[] Vertices { get; } = vertices;
    public D3DBuffer Buffer { get; } = buffer;

    public ID3D11Buffer* Handle = buffer.Handle;
    public uint VertexCount = (uint)vertices.Length;

    public void Dispose()
    {
        Buffer.Dispose();
    }
}

internal unsafe class DebugWindow : MirrorWindow
{
    protected override Vector2 MinSize      { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize      { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize  { get; } = new Vector2(800, 400);

    private readonly CameraHandler CameraHandler;
    private readonly TextureHooker TextureHooker;

    private readonly VertexBuffer  VertexBuffer;

    private readonly RenderTarget  TestRenderTarget;

    private readonly ID3D11DeviceContext* Context;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;

        TestRenderTarget = new RenderTarget((ID3D11Device*)Device.Instance()->D3D11Forwarder, 500, 500);

        Context = ((ID3D11DeviceContext*)Device.Instance()->D3D11DeviceContext);

        Open();
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe struct RenderTargetManagerExtended
    {
        public static RenderTargetManagerExtended* Instance() => (RenderTargetManagerExtended*)RenderTargetManager.Instance();

        [FieldOffset(624 + 32 * 8)]
        public unsafe Texture* DrawTexture;

        [FieldOffset(0x280)]
        public unsafe Texture* EdgeShadows;

        [FieldOffset(0x110)]
        public unsafe Texture* OpaguePass;

        [FieldOffset(0x118)]
        public unsafe Texture* SemiTrans;

        [FieldOffset(0x648)] // Motion blur buffer
        public unsafe Texture* MotionBlurBuffer;

        [FieldOffset(0x118)] // Motion blur buffer
        public unsafe Texture* Unk640;

        [FieldOffset(32 + 10 * 8)]
        public unsafe Texture* DepthStencilTexture;

    }

    ImTextureID? tId;

    private Box ComputeCopyBox(FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Texture* gameRenderTexture, RenderTarget renderTarget)
    {
        return new Box(0, 0, 0, Math.Min(gameRenderTexture->ActualWidth, renderTarget.Width), Math.Min(gameRenderTexture->ActualHeight, renderTarget.Height), 1);
    }

    protected override void OnDraw()
    {
        try
        {

            if (ImGui.Button("Clear"))
            {
                tId = null;
            }

            RenderTargetManagerExtended* rManager = (RenderTargetManagerExtended*)RenderTargetManager.Instance();

            if (rManager != null)
            {
               Texture* uiTexture = rManager->DrawTexture;

                if (uiTexture != null) 
                {
                    //Texture* mainRenderTArget = TestRenderTa;

                    var box = ComputeCopyBox(uiTexture, TestRenderTarget);

                    //Context->CopyResource((ID3D11Resource*)TestRenderTarget.Texture, (ID3D11Resource*)uiTexture->D3D11Texture2D);

                    Context->CopySubresourceRegion((ID3D11Resource*)TestRenderTarget.Texture, 0, 0, 0, 0, (ID3D11Resource*)uiTexture->D3D11Texture2D, 0, ref box);

                    //if (mainRenderTArget != null)
                    {
                        //ID3D11ShaderResourceView* resourceView = (ID3D11ShaderResourceView*)mainRenderTArget->D3D11ShaderResourceView;

                        if (TestRenderTarget.ShaderResourceView != null)
                        {
                            if (tId == null)
                            {
                                tId = new ImTextureID(TestRenderTarget.ShaderResourceView);
                            }

                            ImGui.Image(tId.Value, new Vector2(800, 640));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }


        int camCounter = 0;

        if (ImGui.Button("Flood Camera List"))
        {
            CameraHandler.PrepareCameraList();
        }

        foreach (BaseCamera camera in CameraHandler.Cameras)
        {
            if (ImGui.Button($"[{camCounter}]: {camera.GetType().Name}"))
            {
                CameraHandler.SetActiveCamera(camera);
            }

            ImGui.SameLine();

            if (ImGui.Button($"X##{WindowHandler.InternalCounter}"))
            {
                _ = DalamudServices.Framework.RunOnFrameworkThread(() => CameraHandler.DestroyCamera(camera));
            }

            camCounter++;
        }

        if (ImGui.Button("Spawn Camera"))
        {
            try
            {
                _ = CameraHandler.CreateCamera();
            }
            catch(Exception e)
            {
                MirrorServices.MirrorLog.LogException(e);
            }
        }

        if (ImGui.Button("Set Override Camera"))
        {
            CameraHandler.SetActiveCamera(null);
        }

        if (ImGui.Button("Clear Override Camera"))
        {
            CameraHandler.SetActiveCamera(null);
        }
    }

    protected override void OnDispose()
    {
        TestRenderTarget.Dispose();
    }
}
