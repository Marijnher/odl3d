using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderDevice : IRenderDevice
{
    public Metal.Device Device;

    public string Name => "Metal";
    public IRenderCapabilities Capabilities => new MetalRenderCapabilities();

    public MetalRenderDevice()
    {
        Metal.Load();
        Device = Metal.Device.Default;
    }

    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new MetalRenderSurface(Device, nativeWindow);

    public IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged =>
        new MetalBuffer<T>(Device, description, initialData);

    public ITexture CreateTexture(TextureDescription description, float[]? initialData = null) =>
        throw new NotImplementedException();
    
    public ISampler CreateSampler(SamplerDescription description) =>
        throw new NotImplementedException();

    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        throw new NotImplementedException();

    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        new MetalShaderModule(description);

    public IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description) =>
        new MetalRenderPipeline(Device, description);

    public void Dispose() { }
}