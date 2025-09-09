using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Memory;
using MirrorsEdge.MirrorsEdge.Services;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using KernalDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    protected override Vector2 MinSize      { get; } = new Vector2(350, 136);
    protected override Vector2 MaxSize      { get; } = new Vector2(2000, 2000);
    protected override Vector2 DefaultSize  { get; } = new Vector2(800, 400);

    private readonly CameraHandler  CameraHandler;
    private readonly TextureHooker  TextureHooker;
    private readonly RendererHook   RendererHook;

    private BaseCamera? ActiveCamera;

    private readonly RenderTarget           RenderTarget;
    private readonly RenderTarget           GameRenderTarget;
    private readonly ImTextureID            TextureId;
    private readonly ID3D11Device*          Device;
    private readonly ID3D11DeviceContext*   Context;
    private readonly ID3D11Texture2D*       BackBuffer;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;
        RendererHook  = rendererHook;

        RendererHook.SetRenderPassListener(OnRenderPass);

        Device          = (ID3D11Device*)KernalDevice.Instance()->D3D11Forwarder;

        Context         = (ID3D11DeviceContext*)KernalDevice.Instance()->D3D11DeviceContext;

        RenderTarget    = new RenderTarget(Device, 1920, 1080);

        TextureId       = new ImTextureID(RenderTarget.ShaderResourceView);

        Guid iid        = typeof(ID3D11Texture2D).GUID;

        int hr = ((IDXGISwapChain*)(KernalDevice.Instance()->SwapChain->DXGISwapChain))->GetBuffer(0, &iid, (void**)BackBuffer);

        Open();
    }

    protected override void OnDraw()
    {
        try
        {
            ImGui.Image(TextureId, new Vector2(800, 800));
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

      private bool OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Main)
        {
            try
            {
                //Context->OMSetRenderTargets(1, (ID3D11RenderTargetView**)&RenderTarget.RenderTargetView[0], null);

                //Box box = new Box(0, 0, 0, 1920, 1080, 1);

                //Context->CopyResource((ID3D11Resource*)RenderTarget.Texture, (ID3D11Resource*)BackBuffer);

                //Context->CopySubresourceRegion((ID3D11Resource*)RenderTarget.Texture, 0, 0, 0, 0, (ID3D11Resource*)BackBuffer, 0, ref box);

                
            }
            catch (Exception ex)
            {
                MirrorServices.MirrorLog.LogException(ex);
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
        RenderTarget.Dispose();
    }
}
