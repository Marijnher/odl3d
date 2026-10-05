using System;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// Creates renderer devices for application-owned graphics contexts.
/// </summary>
public static class RenderFactory
{
    /// <summary>
    /// Creates a renderer device for the specified backend, or selects the platform default.
    /// </summary>
    /// <param name="target">The renderer backend to create.</param>
    /// <returns>The created renderer instance.</returns>
    /// <exception cref="RenderException">Thrown when the specified renderer backend is not supported.</exception>
    public static IRenderDevice Create(RenderTarget? target = null) => target switch
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