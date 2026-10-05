using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a rendering device that provides access to GPU resources and capabilities.
/// </summary>
public interface IRenderDevice : IDisposable
{
    /// <summary>
    /// Gets the current render target of the device.
    /// </summary>
    RenderTarget RenderTarget { get; }

    /// <summary>
    /// Gets the name of the rendering device.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the rendering capabilities of the device.
    /// </summary>
    IRenderCapabilities Capabilities { get; }

    /// <summary>
    /// Creates a rendering surface for the specified native window.
    /// </summary>
    /// <param name="nativeWindow">The handle to the native window for which to create the rendering surface.</param>
    /// <returns>The created rendering surface associated with the specified native window.</returns>
    IRenderSurface CreateSurface(nint nativeWindow);

    /// <summary>
    /// Creates a rendering surface that is not attached to a window. Frames acquired from it render into an
    /// RGBA8 color target with a <see cref="TextureFormat.Depth32Float"/> depth target, and can be read back
    /// with <see cref="IRenderFrame.ReadPixels"/>.
    /// </summary>
    /// <param name="width">The width of the surface in pixels. Must be greater than zero.</param>
    /// <param name="height">The height of the surface in pixels. Must be greater than zero.</param>
    /// <returns>The created offscreen rendering surface.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/> is zero.</exception>
    IRenderSurface CreateOffscreenSurface(uint width, uint height);

    /// <summary>
    /// Creates a buffer resource on the GPU with the specified description and optional initial data.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the buffer.</typeparam>
    /// <param name="description">The description of the buffer to create.</param>
    /// <param name="initialData">Optional initial data to populate the buffer with.</param>
    /// <returns>The created buffer resource.</returns>
    IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged;

    /// <summary>
    /// Creates a texture resource on the GPU with the specified description and optional initial data.
    /// </summary>
    /// <param name="description">The description of the texture to create.</param>
    /// <param name="initialData">Optional initial data to populate the texture with.</param>
    /// <returns>The created texture resource.</returns>
    ITexture CreateTexture(TextureDescription description, byte[]? initialData = null);
    
    /// <summary>
    /// Creates a sampler resource on the GPU with the specified description.
    /// </summary>
    /// <param name="description">The description of the sampler to create. If null, a default sampler is created.</param>
    /// <returns>The created sampler resource.</returns>
    ISampler CreateSampler(SamplerDescription? description = null);

    /// <summary>
    /// Creates a depth-stencil state resource on the GPU with the specified description.
    /// </summary>
    /// <param name="description">The description of the depth-stencil state to create.</param>
    /// <returns>The created depth-stencil state resource.</returns>
    IDepthStencilState CreateDepthStencilState(DepthStencilDescription description);

    /// <summary>
    /// Creates a shader module resource on the GPU with the specified description.
    /// </summary>
    /// <param name="description">The description of the shader module to create.</param>
    /// <returns>The created shader module resource.</returns>
    IShaderModule CreateShaderModule(ShaderModuleDescription description);

    /// <summary>
    /// Creates a render pipeline resource on the GPU with the specified description.
    /// </summary>
    /// <param name="description">The description of the render pipeline to create.</param>
    /// <returns>The created render pipeline resource.</returns>
    IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description);
}