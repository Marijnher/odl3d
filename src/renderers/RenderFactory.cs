using System;

namespace odl3d;

/// <summary>
/// Provides a factory for creating and managing instances of the <see cref="IRenderer"/> interface, allowing for the selection and initialization of different rendering backends (e.g., OpenGL) based on a specified identifier. The factory ensures that only one instance of the renderer is created and provides access to it through the <see cref="Renderer"/> property.
/// </summary>
public static class RenderFactory
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="IRenderer"/> interface, which represents the active renderer used for drawing 3D objects in the scene. If the renderer has not been initialized, an exception is thrown, indicating that the <see cref="Create(string)"/> method must be called first to create and configure the renderer. This property provides access to the renderer for rendering operations, such as drawing meshes, setting uniforms, and managing GPU resources.
    /// </summary>
    public static IRenderer Renderer => _instance ?? throw new RenderException("Renderer has not been initialized. Call RenderFactory.Create() first.");
    private static IRenderer? _instance;

    /// <summary>
    /// Creates and initializes a new instance of the <see cref="IRenderer"/> interface based on the specified identifier, allowing for the selection of different rendering backends (e.g., OpenGL). The method sets the singleton instance of the renderer, which can be accessed through the <see cref="Renderer"/> property. If an unknown identifier is provided, a <see cref="RenderException"/> is thrown, indicating that the specified renderer is not supported. This method must be called before accessing the <see cref="Renderer"/> property to ensure that a valid renderer instance is available for rendering operations.
    /// </summary>
    /// <param name="id">The identifier of the renderer to create.</param>
    /// <returns>The created renderer instance.</returns>
    /// <exception cref="RenderException">Thrown when the specified renderer identifier is not supported.</exception>
    public static IRenderer Create(string id)
    {
        return _instance = id switch
        {
            "opengl" => CreateOpenGLRenderer(),
            "metal" => CreateMetalRenderer(),
            _ => throw new RenderException($"Unknown renderer id: {id}")
        };
    }

    private static IRenderer CreateOpenGLRenderer()
    {
        return new Renderers.OpenGL();
    }

    private static IRenderer CreateMetalRenderer()
    {
        return new Renderers.Metal();
    }
}