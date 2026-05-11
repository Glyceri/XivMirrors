using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Services.Interfaces;

namespace TheMirrorsEdge.Services.ChildServices;

public class StatePreserver : IStatePreserver
{
    private const int               BufferSize              = 10;
    
    private VertexShader?           OldVertexShader         = null;
    private PixelShader?            OldPixelShader          = null;
    private PrimitiveTopology       PrimitiveTopology       =  PrimitiveTopology.TriangleList;
    
    private Buffer?[]               VertexBuffers           = new Buffer?[BufferSize];
    private int[]                   StridesRef              = new int[BufferSize];
    private int[]                   OffsetsRef              = new int[BufferSize];
    
    private Buffer?                 IndexBuffer             = null;
    private Format                  Format                  = Format.A8_UNorm;
    private int                     Offset                  = 0;
    
    private Buffer?[]               VertexConstantBuffers   = new Buffer?[BufferSize];
    private Buffer?[]               PixelConstantBuffers    = new Buffer?[BufferSize];
    
    private SamplerState?[]         VertexSamplers          = new SamplerState?[BufferSize];
    private SamplerState?[]         PixelSamplers           = new SamplerState?[BufferSize];
    
    private InputLayout?            OldLayout               = null;
    
    private ShaderResourceView?[]   VertexSRVs              = new ShaderResourceView?[BufferSize];
    private ShaderResourceView?[]   PixelSRVs               = new ShaderResourceView?[BufferSize];
    
    private RawColor4               BlendFactor             = new RawColor4();
    private int                     SampleMask              = 0;
    
    private BlendState?             BlendState              = null;
    private DepthStencilState?      DepthStencilState       = null;
    private RasterizerState?        RasterizerState         = null;
    
    private RawViewportF[]          Viewports               = [];
    private RenderTargetView?[]     RenderTargetViews       = [];
    private DepthStencilView?       DepthStencilView        = null;
    
    private bool                    hasBound                = false;
    
    private readonly DirectXData    DirectXData;
    
    public StatePreserver(DirectXData directXData)
        => DirectXData = directXData;
    
    public void PreserveState()
    {
        OldVertexShader         = DirectXData.Context.VertexShader.Get();
        OldPixelShader          = DirectXData.Context.PixelShader.Get();
        
        PrimitiveTopology       = DirectXData.Context.InputAssembler.PrimitiveTopology;
        
        VertexConstantBuffers   = DirectXData.Context.VertexShader.GetConstantBuffers(0, BufferSize);
        PixelConstantBuffers    = DirectXData.Context.PixelShader.GetConstantBuffers(0, BufferSize);
        
        VertexSamplers          = DirectXData.Context.VertexShader.GetSamplers(0, BufferSize);
        PixelSamplers           = DirectXData.Context.PixelShader.GetSamplers(0, BufferSize);
        
        OldLayout               = DirectXData.Context.InputAssembler.InputLayout;
        
        VertexSRVs              = DirectXData.Context.VertexShader.GetShaderResources(0, BufferSize);
        PixelSRVs               = DirectXData.Context.PixelShader.GetShaderResources(0, BufferSize);
        
        DepthStencilState       = DirectXData.Context.OutputMerger.DepthStencilState;
        RasterizerState         = DirectXData.Context.Rasterizer.State;
        
        DirectXData.Context.InputAssembler.GetVertexBuffers(0, BufferSize, VertexBuffers, StridesRef, OffsetsRef);
        DirectXData.Context.InputAssembler.GetIndexBuffer(out IndexBuffer, out Format, out Offset);
        
        BlendState               = DirectXData.Context.OutputMerger.GetBlendState(out BlendFactor, out SampleMask);
        
        Viewports                = DirectXData.Context.Rasterizer.GetViewports<RawViewportF>();
        
        RenderTargetViews        = DirectXData.Context.OutputMerger.GetRenderTargets(BufferSize, out DepthStencilView);
        
        hasBound = true;
    }
    
    public void RestoreState()
    {
        if (!hasBound)
        {
            return;
        }
        
        hasBound = false;
        
        DirectXData.Context.VertexShader.Set(OldVertexShader);
        DirectXData.Context.PixelShader.Set(OldPixelShader);
        
        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology;
        
        DirectXData.Context.VertexShader.SetConstantBuffers(0, GetCount(VertexConstantBuffers), VertexConstantBuffers);
        DirectXData.Context.PixelShader.SetConstantBuffers(0, GetCount(PixelConstantBuffers), PixelConstantBuffers);
        
        DirectXData.Context.VertexShader.SetSamplers(0, GetCount(VertexSamplers), VertexSamplers);
        DirectXData.Context.PixelShader.SetSamplers(0, GetCount(PixelSamplers), PixelSamplers);
        
        DirectXData.Context.InputAssembler.InputLayout = OldLayout;
        
        DirectXData.Context.VertexShader.SetShaderResources(0, GetCount(VertexSRVs), VertexSRVs);
        DirectXData.Context.PixelShader.SetShaderResources(0, GetCount(PixelSRVs), PixelSRVs);
        
        DirectXData.Context.OutputMerger.DepthStencilState = DepthStencilState;
        DirectXData.Context.Rasterizer.State = RasterizerState;
        
        DirectXData.Context.InputAssembler.SetVertexBuffers(0, GetVertexBufferBindings());
        DirectXData.Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format, Offset);
        
        DirectXData.Context.OutputMerger.SetBlendState(BlendState, BlendFactor, SampleMask);
        
        DirectXData.Context.OutputMerger.SetRenderTargets(DepthStencilView, RenderTargetViews);
        
        DirectXData.Context.Rasterizer.SetViewports(Viewports, Viewports.Length);
    }
    
    private VertexBufferBinding[] GetVertexBufferBindings()
    {
        int count = GetCount(VertexBuffers);
        
        VertexBufferBinding[] vertexBufferBindings = new VertexBufferBinding[count];
        
        for (int i = 0; i < count; i++)
        {
            vertexBufferBindings[i] = new VertexBufferBinding(VertexBuffers[i], StridesRef[i], OffsetsRef[i]);
        }
        
        return vertexBufferBindings;
    }
    
    private int GetCount<T>(T?[] array)
    {
        int count = 0;
        
        for (int i = 0; i < array.Length; i++)
        {
            T? item = array[i];
            
            if (item == null)
            {
                continue;
            }
            
            count++;
        }
        
        return count;
    }
}