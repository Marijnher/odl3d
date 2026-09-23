using System;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Provides a factory for creating and managing instances of the <see cref="IRenderDevice"/> interface, allowing for the selection and initialization of different rendering backends (e.g., OpenGL) based on a specified identifier. The factory ensures that only one instance of the renderer is created and provides access to it through the <see cref="Renderer"/> property.
/// </summary>
public static class RenderFactory
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="IRenderDevice"/> interface, which represents the active renderer used for drawing 3D objects in the scene. If the renderer has not been initialized, an exception is thrown, indicating that the <see cref="Create(RenderTarget)"/> method must be called first to create and configure the renderer. This property provides access to the renderer for rendering operations, such as drawing meshes, setting uniforms, and managing GPU resources.
    /// </summary>
    public static IRenderDevice Renderer => _instance ?? throw new RenderException("Renderer has not been initialized. Call RenderFactory.Create() first.");
    private static IRenderDevice? _instance;

    /// <summary>
    /// Creates and initializes a new instance of the <see cref="IRenderDevice"/> interface based on the specified identifier, allowing for the selection of different rendering backends (e.g., OpenGL). The method sets the singleton instance of the renderer, which can be accessed through the <see cref="Renderer"/> property. If an unknown identifier is provided, a <see cref="RenderException"/> is thrown, indicating that the specified renderer is not supported. This method must be called before accessing the <see cref="Renderer"/> property to ensure that a valid renderer instance is available for rendering operations.
    /// </summary>
    /// <param name="id">The identifier of the renderer to create.</param>
    /// <returns>The created renderer instance.</returns>
    /// <exception cref="RenderException">Thrown when the specified renderer identifier is not supported.</exception>
    public static IRenderDevice Create(RenderTarget? target = null) => _instance = target switch
    {
        RenderTarget.OpenGL => CreateOpenGLRenderer(),
        RenderTarget.Metal => CreateMetalRenderer(),
        null => CreateDefaultRenderer(),
        _ => throw new RenderException($"Unknown renderer target: {target}")
    };

    /// <summary>
    /// Creates and initializes the default renderer based on the current operating system. On macOS, it creates a Metal renderer, while on other platforms, it creates an OpenGL renderer. This method is used when no specific renderer target is provided.
    /// </summary>
    /// <returns>The created default renderer instance based on the current operating system.</returns>
    private static IRenderDevice CreateDefaultRenderer()
    {
        if (OperatingSystem.IsMacOS()) return CreateMetalRenderer();
        return CreateOpenGLRenderer();
    }

    /// <summary>
    /// Creates and initializes an OpenGL renderer instance. This method is used when the OpenGL rendering backend is explicitly requested.
    /// </summary>
    /// <returns>The created OpenGL renderer instance.</returns>
    private static IRenderDevice CreateOpenGLRenderer() => new Renderer.OpenGLAdapter.OpenGLRenderDevice();

    /// <summary>
    /// Creates and initializes a Metal renderer instance. This method is used when the Metal rendering backend is explicitly requested.
    /// </summary>
    /// <returns>The created Metal renderer instance.</returns>
    private static IRenderDevice CreateMetalRenderer() => new Renderer.MetalAdapter.MetalRenderDevice();
}

/// <summary>
/// Represents the available rendering backend targets that can be used to create renderer instances.
/// </summary>
public enum RenderTarget
{
    /// <summary>
    /// Represents the OpenGL rendering backend target.
    /// </summary>
    OpenGL,
    
    /// <summary>
    /// Represents the Metal rendering backend target.
    /// </summary>
    Metal
}