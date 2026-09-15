using System;

namespace odl3d.Renderer;

public interface IRenderDevice : IDisposable
{
    string Name { get; }

    IRenderCapabilities Capabilities { get; }

    IRenderSurface CreateSurface(nint nativeWindow);

    IBuffer CreateBuffer(BufferDescription description, ReadOnlySpan<byte> initialData = default);

    ITexture CreateTexture(TextureDescription description, ReadOnlySpan<byte> initialData = default);
    
    ISampler CreateSampler(SamplerDescription description);

    IDepthStencilState CreateDepthStencilState(DepthStencilDescription description);

    IShaderModule CreateShaderModule(ShaderModuleDescription description);

    IRenderPipeline CreateShaderPipeline(RenderPipelineDescription description);
}