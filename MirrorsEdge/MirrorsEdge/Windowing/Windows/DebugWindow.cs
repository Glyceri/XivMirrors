using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin.Services;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Hooking.Enum;
using MirrorsEdge.MirrorsEdge.Hooking.HookableElements;
using MirrorsEdge.MirrorsEdge.Services;
using MirrorsEdge.MirrorsEdge.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using Device = SharpDX.Direct3D11.Device;
using KernelDevice = FFXIVClientStructs.FFXIV.Client.Graphics.Kernel.Device;

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    private bool _disposed = false;

    protected override System.Numerics.Vector2 MinSize      { get; } = new System.Numerics.Vector2(350, 136);
    protected override System.Numerics.Vector2 MaxSize      { get; } = new System.Numerics.Vector2(2000, 2000);
    protected override System.Numerics.Vector2 DefaultSize  { get; } = new System.Numerics.Vector2(800, 400);

    private readonly CameraHandler  CameraHandler;
    private readonly TextureHooker  TextureHooker;
    private readonly RendererHook   RendererHook;
    private readonly ShaderFactory  ShaderFactory;

    private BaseCamera? ActiveCamera;

    private Device?    Device;
    private SwapChain? SwapChain;
    
    private ShaderResourceView? BackBufferResourceView;
    private Texture2D?          BackBufferTexture;   
    private RenderTargetView?   BackBufferRenderTargetView;

    private ShaderResourceView? BackBufferResourceViewCopy;
    private Texture2D?          BackBufferTextureCopy;
    private RenderTargetView?   BackBufferRenderTargetViewCopy;

    private readonly IDalamudTextureWrap? FullScreenTexture;

    private readonly VertexShader?  VertexShader;
    private readonly PixelShader?   PixelShader;
    private readonly InputLayout?   InputLayout;
    private readonly SamplerState?  SamplerState;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, TextureHooker textureHooker, RendererHook rendererHook, ShaderFactory shaderFactory) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        TextureHooker = textureHooker;
        RendererHook  = rendererHook;
        ShaderFactory = shaderFactory;

        DalamudServices.Framework.Update += Update;

        ImGuiViewportTextureArgs args = new ImGuiViewportTextureArgs()
        {
            AutoUpdate = true,
            KeepTransparency = false,
            TakeBeforeImGuiRender = true,
            ViewportId = ImGui.GetMainViewport().ID,
        };

        RendererHook.SetRenderPassListener(OnRenderPass, OnPostRender);

        //FullScreenTexture = DalamudServices.TextureProvider.CreateFromImGuiViewportAsync(args).Result;

        try
        {
            KernelDevice* kernelDevice = KernelDevice.Instance();

            SwapChain   = new SwapChain((nint)kernelDevice->SwapChain->DXGISwapChain);
            Device      = SwapChain.GetDevice<Device>();

            _ = ShaderFactory.GetVertexShader(Device, "VertexShader.hlsl", [], out VertexShader, out InputLayout, out SamplerState);
            _ = ShaderFactory.GetFragmentShader(Device, "FragmentShader.hlsl", out PixelShader);

        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogError(ex, $"Failed to initialize device.");
        }

        Open();
    }

    private void Update(IFramework framework)
    {
        CameraHandler.SetActiveCamera(CameraHandler.Cameras[1]);

        DalamudServices.Framework.RunOnFrameworkThread(() => CameraHandler.SetActiveCamera(CameraHandler.Cameras[0]));
    }

    private void CreateRenderTarget(Size2 size)
    {
        if (_disposed)
        {
            return;
        }

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
                ImGui.Image(new ImTextureID(BackBufferResourceView.NativePointer), new(size.Width, size.Height));
            }

            if (FullScreenTexture != null)
            {
                ImGui.Image(FullScreenTexture.Handle, new(size.Width, size.Height));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    protected override void OnDraw()
    {
        if (_disposed)
        {
            return;
        }

        //GetBackBuffer();

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

                CameraHandler.SetActiveCamera(ActiveCamera);

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

    private void GetBackBuffer()
    {
        if (_disposed)
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

        CreateRenderTarget(new Size2(1920, 1080));

        try
        {
            DeviceContext context = Device.ImmediateContext;

            BackBufferResourceViewCopy?.Dispose();
            BackBufferTextureCopy?.Dispose();

            BackBufferResourceViewCopy      = null;
            BackBufferTextureCopy           = null;

            // ---- back buffer ----
            Texture2D backBuffer = SwapChain.GetBackBuffer<Texture2D>(0);

            Texture2DDescription desc = backBuffer.Description;

            desc.BindFlags      = BindFlags.ShaderResource | BindFlags.RenderTarget;
            desc.Usage          = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            BackBufferTextureCopy = new Texture2D(Device, desc);

            context.CopyResource(backBuffer, BackBufferTextureCopy);

            BackBufferResourceViewCopy = new ShaderResourceView(Device, BackBufferTextureCopy);
            // ---- end back buffer ----

            context.OutputMerger.SetRenderTargets(BackBufferRenderTargetView!);

            Viewport vp = new Viewport(0, 0, BackBufferTextureCopy.Description.Width, BackBufferTextureCopy.Description.Height);

            context.Rasterizer.SetViewport(vp);

            context.VertexShader.Set(VertexShader);
            context.PixelShader.Set(PixelShader);

            context.PixelShader.SetShaderResource(0, BackBufferResourceViewCopy);
            context.PixelShader.SetSampler(0, SamplerState);

            context.ClearRenderTargetView(BackBufferRenderTargetView, new RawColor4(0, 0, 0, 1.0f));

            BlendStateDescription blendDesc = new BlendStateDescription();

            blendDesc.RenderTarget[0].IsBlendEnabled        = false;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            context.OutputMerger.SetBlendState(new BlendState(Device, blendDesc));

            context.InputAssembler.InputLayout = null;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            context.Draw(3, 0);
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }
    }

    private bool OnRenderPass(RenderPass renderPass)
    {
        if (_disposed)
        {
            return true;
        }

        

        if (renderPass == RenderPass.Main)
        {
          

            return true;
        }

        if (ActiveCamera == CameraHandler.Cameras[1])
        {
            GetBackBuffer();
        }

        return true;
    }

    private bool OnPostRender(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Main)
        {

            
        }
        else
        {
           


        }

        return true;
    }

    protected override void OnDispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        SwapChain   = null;

        BackBufferTexture?.Dispose();
        BackBufferTextureCopy?.Dispose();

        BackBufferRenderTargetView?.Dispose();
        BackBufferRenderTargetViewCopy?.Dispose();  

        InputLayout?.Dispose();
        SamplerState?.Dispose();
        PixelShader?.Dispose();
        VertexShader?.Dispose();

        Device?.Dispose();

        BackBufferResourceView = null;
        BackBufferResourceViewCopy = null;

        BackBufferTexture = null;
        BackBufferTextureCopy = null;

        BackBufferRenderTargetView = null;
        BackBufferRenderTargetViewCopy = null;

        FullScreenTexture?.Dispose();

        RendererHook.SetRenderPassListener(null, null);

        DalamudServices.Framework.Update -= Update;
    }
}
