using Dalamud.Bindings.ImGui;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Services;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.Numerics;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using KernelDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

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

    private readonly Device?    Device;
    private readonly SwapChain? SwapChain;
    
    private ShaderResourceView? BackBufferResourceView;
    private ShaderResourceView? BackBufferResourceViewCopy;

    private Texture2D?          BackBufferTexture;
    private Texture2D?          BackBufferTextureCopy;
    
    private RenderTargetView?   BackBufferRenderTargetView;
    private RenderTargetView?   BackBufferRenderTargetViewCopy;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;
        RendererHook  = rendererHook;

        RendererHook.SetRenderPassListener(OnRenderPass);

        try
        {
            KernelDevice* kernelDevice = KernelDevice.Instance();

            SwapChain   = new SwapChain((nint)kernelDevice->SwapChain->DXGISwapChain);
            Device      = SwapChain.GetDevice<Device>();
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, $"Failed to initialize device.");
        }

        Open();
    }

    private void CreateRenderTarget(Size2 size)
    {
        if (BackBufferTexture != null &&
            BackBufferTexture.Description.Width == size.Width &&
            BackBufferTexture.Description.Height == size.Height)
        {
            return;
        }

        try
        {
            BackBufferResourceView?.Dispose();
            BackBufferTexture?.Dispose();
            BackBufferRenderTargetView?.Dispose();

            BackBufferResourceViewCopy?.Dispose();
            BackBufferTextureCopy?.Dispose();
            BackBufferRenderTargetViewCopy?.Dispose();

            Texture2DDescription texture2DDescription = new Texture2DDescription
            {
                Width               = size.Width,
                Height              = size.Height,
                MipLevels           = 1,
                ArraySize           = 1,
                Format              = Format.R8G8B8A8_UNorm,
                SampleDescription   = new SampleDescription(1, 0),
                Usage               = ResourceUsage.Default,
                BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags      = CpuAccessFlags.None,
                OptionFlags         = ResourceOptionFlags.None,
            };

            BackBufferTexture               = new Texture2D(Device, texture2DDescription);
            BackBufferRenderTargetView      = new RenderTargetView(Device, BackBufferTexture);
            BackBufferResourceView          = new ShaderResourceView(Device, BackBufferTexture);

            BackBufferTextureCopy           = new Texture2D(Device, texture2DDescription);
            BackBufferRenderTargetViewCopy  = new RenderTargetView(Device, BackBufferTextureCopy);
            BackBufferResourceViewCopy      = new ShaderResourceView(Device, BackBufferTextureCopy);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    private void DrawBackBuffer()
    {
        try
        {
            Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
            Size2 size = new Size2((int)contentRegionAvail.X, (int)contentRegionAvail.Y);

            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            /*
            if (Device == null)
            {
                return;
            }

            if (SwapChain == null)
            {
                return;
            }

            DeviceContext context = Device.ImmediateContext;

            BackBufferResourceViewCopy?.Dispose();
            BackBufferTextureCopy?.Dispose();
            BackBufferResourceViewCopy = null;
            BackBufferTextureCopy = null;

            Texture2D               backBuffer              = SwapChain.GetBackBuffer<Texture2D>(0);
            Texture2DDescription    backBufferDescription   = backBuffer.Description;

            backBufferDescription.BindFlags         = BindFlags.ShaderResource | BindFlags.RenderTarget;
            backBufferDescription.Usage             = ResourceUsage.Default;
            backBufferDescription.CpuAccessFlags    = CpuAccessFlags.None;
            backBufferDescription.OptionFlags       = ResourceOptionFlags.None;

            BackBufferTextureCopy = new Texture2D(Device, backBufferDescription);

            context.CopyResource(backBuffer, BackBufferTextureCopy);

            BackBufferResourceViewCopy = new ShaderResourceView(Device, BackBufferTextureCopy);

            ImGui.Image(new ImTextureID(BackBufferResourceViewCopy.NativePointer), new(size.Width, size.Height));
            */

            var texture = default(ComPtr<ID3D11Texture2D>);

            var device = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device.Instance();

            Guid guid = typeof(Texture2D).GUID;

            //fixed (Guid* piid = &guid)
            {
                ((IDXGISwapChain*)device->SwapChain->DXGISwapChain)->GetBuffer(0, &guid, (void**)texture.GetAddressOf());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnDraw()
    {
        DrawBackBuffer();

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
        BackBufferResourceView?.Dispose();
        BackBufferResourceViewCopy?.Dispose();

        BackBufferTexture?.Dispose();
        BackBufferTextureCopy?.Dispose();

        BackBufferRenderTargetView?.Dispose();
        BackBufferRenderTargetViewCopy?.Dispose();
    }
}
