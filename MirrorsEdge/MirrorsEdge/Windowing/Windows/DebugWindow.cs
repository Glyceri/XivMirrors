using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Services;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using Device = SharpDX.Direct3D11.Device;
using KernelDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    protected override System.Numerics.Vector2 MinSize      { get; } = new System.Numerics.Vector2(350, 136);
    protected override System.Numerics.Vector2 MaxSize      { get; } = new System.Numerics.Vector2(2000, 2000);
    protected override System.Numerics.Vector2 DefaultSize  { get; } = new System.Numerics.Vector2(800, 400);

    private readonly CameraHandler  CameraHandler;
    private readonly TextureHooker  TextureHooker;
    private readonly RendererHook   RendererHook;

    private BaseCamera? ActiveCamera;

    private readonly Device?    Device;
    private readonly SwapChain? SwapChain;
    
    private ShaderResourceView? BackBufferResourceView;
    private Texture2D?          BackBufferTexture;   
    private RenderTargetView?   BackBufferRenderTargetView;

    private readonly IDalamudTextureWrap FullScreenTexture;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;
        RendererHook  = rendererHook;

        RendererHook.SetRenderPassListener(OnRenderPass);

        ImGuiViewportTextureArgs args = new ImGuiViewportTextureArgs()
        {
            AutoUpdate = true,
            KeepTransparency = false,
            TakeBeforeImGuiRender = true,
            ViewportId = ImGui.GetMainViewport().ID,
        };

        FullScreenTexture = DalamudServices.TextureProvider.CreateFromImGuiViewportAsync(args).Result;

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
            System.Numerics.Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
            Size2 size = new Size2((int)contentRegionAvail.X, (int)contentRegionAvail.Y);

            if (size.Width <= 0 || size.Height <= 0)
            {
                return;
            }

            if (Device == null)
            {
                return;
            }

            if (SwapChain == null)
            {
                return;
            }

            if (BackBufferResourceView != null)
            {
                if (!BackBufferResourceView.IsDisposed)
                {
                    ImGui.Image(new ImTextureID(BackBufferResourceView.NativePointer), new(size.Width, size.Height));
                }
            }

            ImGui.Image(FullScreenTexture.Handle, new(size.Width, size.Height));
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

    public void StripAlpha(DeviceContext context)
    {
        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled = true;
        blendDesc.RenderTarget[0].SourceBlend = BlendOption.One;
        blendDesc.RenderTarget[0].DestinationBlend = BlendOption.Zero;
        blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
        blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
        blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
        blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
        blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        BlendState blendState = new BlendState(Device, blendDesc);

        context.OutputMerger.SetBlendState(blendState, null, -1);
    }

    private bool OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Main)
        {
            return true;

            try
            {
                DeviceContext context = Device.ImmediateContext;

                BackBufferResourceView?.Dispose();
                BackBufferTexture?.Dispose();
                BackBufferResourceView = null;
                BackBufferTexture = null;

                using (Texture2D            backBuffer    = SwapChain.GetBackBuffer<Texture2D>(0))
                using (ShaderResourceView   backBufferSRV = new ShaderResourceView(Device, backBuffer))
                using (RenderTargetView     backBufferRTV = new RenderTargetView(Device, BackBufferTexture))
                {
                    Texture2DDescription backBufferDescription = backBuffer.Description;

                    backBufferDescription.BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget;
                    backBufferDescription.Usage = ResourceUsage.Default;
                    backBufferDescription.CpuAccessFlags = CpuAccessFlags.None;
                    backBufferDescription.OptionFlags = ResourceOptionFlags.None;

                    BackBufferTexture = new Texture2D(Device, backBufferDescription);

                    context.OutputMerger.SetRenderTargets(BackBufferRenderTargetView);

                    context.PixelShader.SetShaderResource(0, backBufferSRV);
                    //context.PixelShader.Set(pixelShaderThatStripsAlpha);
                    //context.VertexShader.Set(fullscreenVS);

                    context.Draw(3, 0);

                    context.CopyResource(backBuffer, BackBufferTexture);

                    BackBufferResourceView = new ShaderResourceView(Device, BackBufferTexture);
                }
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
        BackBufferTexture?.Dispose();
        BackBufferRenderTargetView?.Dispose();

        FullScreenTexture?.Dispose();
    }
}
