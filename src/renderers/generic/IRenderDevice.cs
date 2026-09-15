using System;

namespace odl3d.Renderer;

public interface IRenderDevice : IDisposable
{
    IRenderCapabilities Capabilities { get; }

    IRenderSurface CreateSurface(nint nativeWindow);

    IBuffer CreateBuffer(BufferDescription bufferDescription, ReadOnlySpan<byte> initialData = default);

    ITexture CreateTexture(TextureDescription textureDescription, ReadOnlySpan<byte> initialData = default);
    
    ISampler CreateSampler(SamplerDescription samplerDescription);

    IShaderModule CreateShaderModule(ShaderModuleDescription description);

    IRenderPipeline CreateShaderPipeline(RenderPipelineDescription description);
}