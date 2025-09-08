using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Memory;
using MirrorsEdge.MirrorsEdge.Services;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

public struct Vertex
{
    public Vector3D<float> Position;
    public Vector2D<float> UV;
    public Vertex(Vector3D<float> position, Vector2D<float> uv)
    {
        Position = position;
        UV = uv;
    }
}

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

    private readonly CameraHandler  CameraHandler;
    private readonly TextureHooker  TextureHooker;
    private readonly RendererHook   RendererHook;

    private readonly VertexBuffer  VertexBuffer;

    private readonly RenderTarget  TestRenderTarget;

    private readonly ID3D11DeviceContext* Context;

    private BaseCamera? ActiveCamera;

    private IDalamudTextureWrap? Wrap;

    private Texture* tt;

    ID3D11BlendState* blendState;
    private ID3D11SamplerState* samplerState = null;


    private VertexBuffer? squareBuffer;

    private ID3D11Texture2D* TempTexture;

    private ID3D11ShaderResourceView* ShaderResourceView;

    private unsafe ID3D11BlendState* CreateBlendState(RenderTargetBlendDesc renderTargetBlendDesc)
    {
        var description = new BlendDesc(
            alphaToCoverageEnable: false,
            independentBlendEnable: false
        );
        description.RenderTarget[0] = renderTargetBlendDesc;
        ID3D11BlendState* state = null;
        _ = ((ID3D11Device*)Device.Instance()->D3D11Forwarder)->CreateBlendState(ref description, &state);
        return state;
    }

    public static List<Vertex> Plane(Matrix4X4<float>? positionTransform = null, Matrix4X4<float>? uvTransform = null)
    {
        var tl = new Vertex(
            Vector3D.Transform(new Vector3D<float>(-1, 1, 0), positionTransform ?? Matrix4X4<float>.Identity),
            Vector2D.Transform(new Vector2D<float>(0, 0), uvTransform ?? Matrix4X4<float>.Identity));
        var tr = new Vertex(
            Vector3D.Transform(new Vector3D<float>(1, 1, 0), positionTransform ?? Matrix4X4<float>.Identity),
            Vector2D.Transform(new Vector2D<float>(1, 0), uvTransform ?? Matrix4X4<float>.Identity));
        var bl = new Vertex(
            Vector3D.Transform(new Vector3D<float>(-1, -1, 0), positionTransform ?? Matrix4X4<float>.Identity),
            Vector2D.Transform(new Vector2D<float>(0, 1), uvTransform ?? Matrix4X4<float>.Identity));
        var br = new Vertex(
            Vector3D.Transform(new Vector3D<float>(1, -1, 0), positionTransform ?? Matrix4X4<float>.Identity),
            Vector2D.Transform(new Vector2D<float>(1, 1), uvTransform ?? Matrix4X4<float>.Identity));
        return new List<Vertex>(){
            tl,
            tr,
            bl,
            bl,
            tr,
            br,
        };
    }

    private D3DBuffer CreateBuffer(Span<byte> bytes, BindFlag bindFlag)
    {
        fixed (byte* p = bytes)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(bytes));
            }
            SubresourceData subresourceData = new SubresourceData(
                pSysMem: p,
                sysMemPitch: 0,
                sysMemSlicePitch: 0
            );
            BufferDesc description = new BufferDesc(
                byteWidth: (uint)bytes.Length,
                usage: Usage.Dynamic,
                bindFlags: (uint)bindFlag,
                cPUAccessFlags: (uint)CpuAccessFlag.Write,
                miscFlags: 0,
                structureByteStride: 0
            );
            ID3D11Buffer* buffer = null;
            ((ID3D11Device*)Device.Instance()->D3D11Forwarder)->CreateBuffer(ref description, ref subresourceData, ref buffer);
            return new D3DBuffer(buffer, (uint)bytes.Length);
        }
    }

    private VertexBuffer CreateVertexBuffer(List<Vertex> vertices)
    {
        var array = vertices.ToArray();
        var buffer = CreateBuffer(MemoryMarshal.AsBytes(new Span<Vertex>(array)), BindFlag.VertexBuffer);
        return new VertexBuffer(array, buffer);
    }

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;
        RendererHook  = rendererHook;

        RendererHook.SetRenderPassListener(OnRenderPass);

        TestRenderTarget = new RenderTarget((ID3D11Device*)Device.Instance()->D3D11Forwarder, (uint)(1920 * 1.5f), (uint)(1080 * 1.5f));

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

            if (RenderTargetManager.Instance() != null)
            {
                //Texture* uiTexture = RenderTargetManager.Instance()->GBuffers[0];

                

                //if (uiTexture != null) 
                {
                    //Texture* mainRenderTArget = TestRenderTa;

                    //var box = ComputeCopyBox(uiTexture, TestRenderTarget);

                    //MirrorServices.MirrorLog.Log(box.Left + ", " + box.Right + ", " + box.Top + ", " + box.Bottom);

                    //Context->CopyResource((ID3D11Resource*)TestRenderTarget.Texture, (ID3D11Resource*)uiTexture->D3D11Texture2D);

                    //Context->CopySubresourceRegion((ID3D11Resource*)TestRenderTarget.Texture, 0, 0, 0, 0, (ID3D11Resource*)uiTexture->D3D11Texture2D, 0, ref box);

                    //if (mainRenderTArget != null)
                    {
                        //ID3D11ShaderResourceView* resourceView = (ID3D11ShaderResourceView*)mainRenderTArget->D3D11ShaderResourceView;

                        if (ShaderResourceView != null)
                        {
                            if (tId == null)
                            {
                                tId = new ImTextureID(ShaderResourceView);
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
                ActiveCamera = camera;

                //CameraHandler.SetActiveCamera(ActiveCamera);

                if (ActiveCamera == CameraHandler.GameCamera)
                {
                    ActiveCamera = null;
                }
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
    }

    public void SetSampler(ID3D11DeviceContext* context, ID3D11ShaderResourceView* shaderResourceView)
    {
        ID3D11ShaderResourceView** ptr = &shaderResourceView;
        context->PSSetShaderResources(0, 1, ptr);
        context->PSSetSamplers(0, 1, ref samplerState);
    }

    public void DrawSquare(ID3D11DeviceContext* context)
    {
        fixed (ID3D11Buffer** pHandle = &squareBuffer!.Handle)
        {
            uint stride = (uint)sizeof(Vertex);
            uint offsets = 0;
            context->IASetVertexBuffers(0, 1, pHandle, &stride, &offsets);
            context->IASetPrimitiveTopology(Silk.NET.Core.Native.D3DPrimitiveTopology.D3D11PrimitiveTopologyTrianglelist);
            context->Draw(squareBuffer.VertexCount, 0);
        }
    }

    private bool OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Main)
        {
            IDXGISwapChain* swopChain = ((IDXGISwapChain*)Device.Instance()->SwapChain->DXGISwapChain);

            TempTexture = swopChain->GetBuffer<ID3D11Texture2D>(0);

            if (TempTexture != null)
            {
                Context->CopyResource((ID3D11Resource*)TestRenderTarget.Texture, (ID3D11Resource*)TempTexture);
            }

            return true;
        }

        if (ActiveCamera == null)
        {
            return false;
        }

      
        return true;
    }

    protected override void OnDispose()
    {
        Wrap?.Dispose();

        TestRenderTarget.Dispose();
    }
}
