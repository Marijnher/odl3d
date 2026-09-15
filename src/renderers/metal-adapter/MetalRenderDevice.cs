using System;

namespace odl3d.Renderer.MetalAdapter;

public class MetalRenderDevice : IRenderDevice
{
    public string Name => "Metal";

    private Metal.Device Device;

    public IRenderCapabilities Capabilities => new MetalRenderCapabilities();

    public MetalRenderDevice()
    {
        Metal.Load();
        Device = Metal.Device.Default;
    }

    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new MetalRenderSurface(Metal.MetalLayer.AttachToWindow(Device, nativeWindow));

    public IBuffer CreateBuffer(BufferDescription description, ReadOnlySpan<byte> initialData = default) =>
        throw new NotImplementedException();

    public ITexture CreateTexture(TextureDescription description, ReadOnlySpan<byte> initialData = default) =>
        throw new NotImplementedException();
    
    public ISampler CreateSampler(SamplerDescription description) =>
        throw new NotImplementedException();

    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        throw new NotImplementedException();

    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        throw new NotImplementedException();

    public IRenderPipeline CreateShaderPipeline(RenderPipelineDescription description) =>
        throw new NotImplementedException();

    public void Dispose() { }
}