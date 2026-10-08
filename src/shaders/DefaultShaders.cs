namespace odl3d;

/// <summary>
/// Provides access to the default vertex and fragment shaders for different render targets.
/// </summary>
internal static partial class DefaultShaders
{
    /// <summary>
    /// Gets the vertex shader source code for the specified render target.
    /// </summary>
    /// <param name="target">The render target for which to get the vertex shader source code.</param>
    /// <returns>The vertex shader source code as a string.</returns>
    /// <exception cref="RenderException"></exception>
    public static string Vertex(RenderTarget target) => target switch
    {
        RenderTarget.OpenGL => OpenGLVertex,
        RenderTarget.Metal => MetalVertex,
        _ => throw new RenderException($"Unsupported render target: {target}.")
    };

    /// <summary>
    /// Gets the fragment shader source code for the specified render target.
    /// </summary>
    /// <param name="target">The render target for which to get the fragment shader source code.</param>
    /// <returns>The fragment shader source code as a string.</returns>
    /// <exception cref="RenderException"></exception>
    public static string Fragment(RenderTarget target) => target switch
    {
        RenderTarget.OpenGL => OpenGLFragment,
        RenderTarget.Metal => MetalFragment,
        _ => throw new RenderException($"Unsupported render target: {target}.")
    };
}