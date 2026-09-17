using System;

namespace odl3d.Renderer;

public interface IRenderDevice : IDisposable
{
    string Name { get; }

    IRenderCapabilities Capabilities { get; }

    IRenderSurface CreateSurface(nint nativeWindow);

    IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged;

    ITexture CreateTexture(TextureDescription description, byte[]? initialData = null);
    
    ISampler CreateSampler(SamplerDescription? description = null);

    IDepthStencilState CreateDepthStencilState(DepthStencilDescription description);

    IShaderModule CreateShaderModule(ShaderModuleDescription description);

    IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description);
}