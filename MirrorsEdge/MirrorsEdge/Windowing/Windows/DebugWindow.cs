using Dalamud.Bindings.ImGui;
using MirrorsEdge.mirrorsedge.Hooking.HookableElements;
using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.MirrorsEdge.Cameras;
using MirrorsEdge.MirrorsEdge.Cameras.CameraTypes;
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

namespace MirrorsEdge.MirrorsEdge.Windowing.Windows;

internal unsafe class DebugWindow : MirrorWindow
{
    private bool _disposed = false;

    protected override System.Numerics.Vector2 MinSize      { get; } = new System.Numerics.Vector2(350, 136);
    protected override System.Numerics.Vector2 MaxSize      { get; } = new System.Numerics.Vector2(2000, 2000);
    protected override System.Numerics.Vector2 DefaultSize  { get; } = new System.Numerics.Vector2(800, 400);

    private readonly CameraHandler  CameraHandler;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;
    private readonly DirectXData    DirectXData;
    
    private ShaderResourceView? BackBufferResourceView;
    private Texture2D?          BackBufferTexture;   
    private RenderTargetView?   BackBufferRenderTargetView;

    private ShaderResourceView? BackBufferResourceViewCopy;
    private Texture2D?          BackBufferTextureCopy;
    private RenderTargetView?   BackBufferRenderTargetViewCopy;

    private System.Numerics.Vector2 _screenSize = System.Numerics.Vector2.Zero;

    public DebugWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices, CameraHandler cameraHandler, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderFactory, DirectXData directXData) : base(windowHandler, dalamudServices, mirrorServices, "Mirrors Dev Window", ImGuiWindowFlags.None)
    {
        CameraHandler = cameraHandler;
        RendererHook  = rendererHook;
        ScreenHook    = screenHook;
        ShaderHandler = shaderFactory;
        DirectXData   = directXData;

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreensizeChanged);
        ScreenHook.SetupSize(ref _screenSize);

        RendererHook.RegisterRenderPassListener(OnRenderPass);

        Open();
    }

    private void OnScreensizeChanged(System.Numerics.Vector2 screenSize)
    {
        _screenSize = screenSize;
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

            BackBufferTexture               = new Texture2D(DirectXData.Device, texture2DDescription);
            BackBufferRenderTargetView      = new RenderTargetView(DirectXData.Device, BackBufferTexture);
            BackBufferResourceView          = new ShaderResourceView(DirectXData.Device, BackBufferTexture);

            BackBufferTextureCopy           = new Texture2D(DirectXData.Device, texture2DDescription);
            BackBufferRenderTargetViewCopy  = new RenderTargetView(DirectXData.Device, BackBufferTextureCopy);
            BackBufferResourceViewCopy      = new ShaderResourceView(DirectXData.Device, BackBufferTextureCopy);
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

            if (BackBufferResourceView != null)
            {
                ImGui.Image(new ImTextureID(BackBufferResourceView.NativePointer), new(size.Width, size.Height));
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
    }

    private void GetBackBuffer()
    {
        if (_disposed)
        {
            return;
        }

        CreateRenderTarget(new Size2((int)_screenSize.X, (int)_screenSize.Y));

        try
        {
            BackBufferResourceViewCopy?.Dispose();
            BackBufferTextureCopy?.Dispose();

            BackBufferResourceViewCopy      = null;
            BackBufferTextureCopy           = null;

            // ---- back buffer ----
            Texture2D backBuffer = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);

            Texture2DDescription desc = backBuffer.Description;

            desc.BindFlags      = BindFlags.ShaderResource | BindFlags.RenderTarget;
            desc.Usage          = ResourceUsage.Default;
            desc.CpuAccessFlags = CpuAccessFlags.None;

            BackBufferTextureCopy = new Texture2D(DirectXData.Device, desc);

            DirectXData.Context.CopyResource(backBuffer, BackBufferTextureCopy);

            BackBufferResourceViewCopy = new ShaderResourceView(DirectXData.Device, BackBufferTextureCopy);
            // ---- end back buffer ----

            DirectXData.Context.OutputMerger.SetRenderTargets(BackBufferRenderTargetView!);

            Viewport vp = new Viewport(0, 0, BackBufferTextureCopy.Description.Width, BackBufferTextureCopy.Description.Height);

            DirectXData.Context.Rasterizer.SetViewport(vp);

            DirectXData.Context.VertexShader.Set(ShaderHandler.VertexShader);
            DirectXData.Context.PixelShader.Set(ShaderHandler.FragmentShader);

            DirectXData.Context.PixelShader.SetShaderResource(0, BackBufferResourceViewCopy);
            DirectXData.Context.PixelShader.SetSampler(0, ShaderHandler.SamplerState);

            DirectXData.Context.ClearRenderTargetView(BackBufferRenderTargetView, new RawColor4(0, 0, 0, 1.0f));

            BlendStateDescription blendDesc = new BlendStateDescription();

            blendDesc.RenderTarget[0].IsBlendEnabled        = false;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

            DirectXData.Context.InputAssembler.InputLayout = null;
            DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            DirectXData.Context.Draw(3, 0);
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }
    }

    private void PreRenderPass()
    {
        if (CameraHandler.ActiveCamera is MirrorCamera { })
        {
            GetBackBuffer();
        }
    }

    private void PostRenderPass()
    {

    }


    private void OnRenderPass(RenderPass renderPass)
    {
        if (_disposed)
        {
            return;
        }

        if (renderPass == RenderPass.Pre)
        {
            PreRenderPass();
        }

        if (renderPass == RenderPass.Post)
        {
            PostRenderPass();
        }
    }

    protected override void OnDispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        BackBufferTexture?.Dispose();
        BackBufferTextureCopy?.Dispose();

        BackBufferRenderTargetView?.Dispose();
        BackBufferRenderTargetViewCopy?.Dispose();  

        BackBufferResourceView = null;
        BackBufferResourceViewCopy = null;

        BackBufferTexture = null;
        BackBufferTextureCopy = null;

        BackBufferRenderTargetView = null;
        BackBufferRenderTargetViewCopy = null;

        RendererHook.DeregisterRenderPassListener(OnRenderPass);

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreensizeChanged);
    }
}
