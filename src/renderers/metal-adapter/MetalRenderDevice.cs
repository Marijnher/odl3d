using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a render device using the Metal graphics API.
/// </summary>
internal class MetalRenderDevice : IRenderDevice
{
    /// <summary>
    /// Gets the render target type used by this device.
    /// </summary>
    public RenderTarget RenderTarget => RenderTarget.Metal;

    /// <summary>
    /// Gets the underlying Metal device instance.
    /// </summary>
    public Metal.Device Device;

    /// <summary>
    /// Gets the name of the render device.
    /// </summary>
    public string Name => "Metal";

    /// <summary>
    /// Gets the rendering capabilities of the device.
    /// </summary>
    public IRenderCapabilities Capabilities => new MetalRenderCapabilities();

    /// <summary>
    /// Gets a value indicating whether the device has been disposed.
    /// </summary>
    public bool Disposed { get; private set;}

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalRenderDevice"/> class.
    /// </summary>
    public MetalRenderDevice()
    {
        Metal.Load();
        Device = Metal.Device.Default;
    }

    /// <summary>
    /// Creates a render surface for the specified native window.
    /// </summary>
    /// <param name="nativeWindow">The native window handle for which to create the render surface.</param>
    /// <returns>The created render surface.</returns>
    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new MetalRenderSurface(Device, nativeWindow);

    /// <summary>
    /// Creates a buffer with the specified description and optional initial data.
    /// </summary>
    /// <typeparam name="T">The type of the buffer elements.</typeparam>
    /// <param name="description">The description of the buffer to create.</param>
    /// <param name="initialData">Optional initial data for the buffer.</param>
    /// <returns>The created buffer.</returns>
    public IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged =>
        new MetalBuffer<T>(Device, description, initialData);

    /// <summary>
    /// Creates a texture with the specified description and optional initial data.
    /// </summary>
    /// <param name="description">The description of the texture to create.</param>
    /// <param name="initialData">Optional initial data for the texture.</param>
    /// <returns>The created texture.</returns>
    public ITexture CreateTexture(TextureDescription description, byte[]? initialData = null) =>
        new MetalTexture(Device, description, initialData);
    
    /// <summary>
    /// Creates a sampler with the specified description.
    /// </summary>
    /// <param name="description">The description of the sampler to create. If null, a default description is used.</param>
    /// <returns>The created sampler.</returns>
    public ISampler CreateSampler(SamplerDescription? description = null) =>
        new MetalSampler(Device, description ?? new SamplerDescription());

    /// <summary>
    /// Creates a depth-stencil state with the specified description.
    /// </summary>
    /// <param name="description">The description of the depth-stencil state to create.</param>
    /// <returns>The created depth-stencil state.</returns>
    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        new MetalDepthStencilState(Device, description);

    /// <summary>
    /// Creates a shader module with the specified description.
    /// </summary>
    /// <param name="description">The description of the shader module to create.</param>
    /// <returns>The created shader module.</returns>
    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        new MetalShaderModule(description);

    /// <summary>
    /// Creates a render pipeline with the specified description.
    /// </summary>
    /// <param name="description">The description of the render pipeline to create.</param>
    /// <returns>The created render pipeline.</returns>
    public IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description) =>
        new MetalRenderPipeline(Device, description);

    /// <summary>
    /// Disposes the render device and releases all associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Device.Dispose();
        Disposed = true;
    }
}