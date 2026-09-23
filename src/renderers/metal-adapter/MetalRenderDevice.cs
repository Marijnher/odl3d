using System;

namespace odl3d.Renderer.MetalAdapter;

internal class MetalRenderDevice : IRenderDevice
{
    public RenderTarget RenderTarget => RenderTarget.Metal;
    public Metal.Device Device;

    public string Name => "Metal";
    public IRenderCapabilities Capabilities => new MetalRenderCapabilities();

    public bool Disposed { get; private set;}

    public MetalRenderDevice()
    {
        Metal.Load();
        Device = Metal.Device.Default;
    }

    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new MetalRenderSurface(Device, nativeWindow);

    public IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged =>
        new MetalBuffer<T>(Device, description, initialData);

    public ITexture CreateTexture(TextureDescription description, byte[]? initialData = null) =>
        new MetalTexture(Device, description, initialData);
    
    public ISampler CreateSampler(SamplerDescription? description = null) =>
        new MetalSampler(Device, description ?? new SamplerDescription());

    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        new MetalDepthStencilState(Device, description);

    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        new MetalShaderModule(description);

    public IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description) =>
        new MetalRenderPipeline(Device, description);

    public void Dispose()
    {
        if (Disposed) return;
        Device.Dispose();
        Disposed = true;
    }
}