using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL render device.
/// </summary>
internal class OpenGLRenderDevice : IRenderDevice
{
    /// <summary>
    /// Gets the render target type of the OpenGL render device.
    /// </summary>
    public RenderTarget RenderTarget => RenderTarget.OpenGL;

    /// <summary>
    /// Gets the name of the OpenGL render device.
    /// </summary>
    public string Name => "OpenGL";

    /// <summary>
    /// Gets the rendering capabilities of the OpenGL render device.
    /// </summary>
    public IRenderCapabilities Capabilities { get; } = new OpenGLRenderCapabilities();

    /// <summary>
    /// Gets a value indicating whether the OpenGL render device has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Gets or sets the current vertex state of the OpenGL render device.
    /// </summary>
    public GLVertexState? VertexState { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLRenderDevice"/> class.
    /// </summary>
    public OpenGLRenderDevice()
    {
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MAJOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_CONTEXT_VERSION_MINOR, 3);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_PROFILE, GLFW.GLFW_OPENGL_CORE_PROFILE);
        GLFW.glfwWindowHint(GLFW.GLFW_OPENGL_FORWARD_COMPAT, GLFW.GLFW_TRUE);
        // GL is initialized in OpenGLRenderSurface after setting the current context.
    }

    /// <summary>
    /// Creates a new render surface for the specified native window.
    /// </summary>
    /// <param name="nativeWindow">The native window handle for which to create the render surface.</param>
    /// <returns>The created render surface.</returns>
    public IRenderSurface CreateSurface(nint nativeWindow) =>
        new OpenGLRenderSurface(this, nativeWindow);

    /// <summary>
    /// Creates a new buffer with the specified description and optional initial data.
    /// </summary>
    /// <typeparam name="T">The type of the buffer elements.</typeparam>
    /// <param name="description">The description of the buffer to create.</param>
    /// <param name="initialData">Optional initial data for the buffer.</param>
    /// <returns>The created buffer.</returns>
    public IBuffer<T> CreateBuffer<T>(BufferDescription description, T[]? initialData = null) where T : unmanaged =>
        new OpenGLBuffer<T>(this, description, initialData);

    /// <summary>
    /// Creates a new texture with the specified description and optional initial data.
    /// </summary>
    /// <param name="description">The description of the texture to create.</param>
    /// <param name="initialData">Optional initial data for the texture.</param>
    /// <returns>The created texture.</returns>
    public ITexture CreateTexture(TextureDescription description, byte[]? initialData = null) =>
        new OpenGLTexture(description, initialData);
    
    /// <summary>
    /// Creates a new sampler with the specified description.
    /// </summary>
    /// <param name="description">The description of the sampler to create. If null, a default description is used.</param>
    /// <returns>The created sampler.</returns>
    public ISampler CreateSampler(SamplerDescription? description = null) =>
        new OpenGLSampler(description ?? new SamplerDescription());

    /// <summary>
    /// Creates a new depth-stencil state with the specified description.
    /// </summary>
    /// <param name="description">The description of the depth-stencil state to create.</param>
    /// <returns>The created depth-stencil state.</returns>
    public IDepthStencilState CreateDepthStencilState(DepthStencilDescription description) =>
        new OpenGLDepthStencilState(description);

    /// <summary>
    /// Creates a new shader module with the specified description.
    /// </summary>
    /// <param name="description">The description of the shader module to create.</param>
    /// <returns>The created shader module.</returns>
    public IShaderModule CreateShaderModule(ShaderModuleDescription description) =>
        new OpenGLShaderModule(description);

    /// <summary>
    /// Creates a new render pipeline with the specified description.
    /// </summary>
    /// <param name="description">The description of the render pipeline to create.</param>
    /// <returns>The created render pipeline.</returns>
    public IRenderPipeline CreateRenderPipeline(RenderPipelineDescription description) =>
        new OpenGLRenderPipeline(description);

    /// <summary>
    /// Disposes the render device and releases any associated resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Disposed = true;
    }
}