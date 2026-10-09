using System.Collections.Generic;
using odl3d.Renderer;

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

    /// <summary>
    /// Creates a shader pipeline using the default vertex and fragment shaders for the specified renderer.
    /// </summary>
    /// <param name="renderer">The render device for which to create the shader pipeline.</param>
    /// <returns>A new shader pipeline using the default vertex and fragment shaders for the specified renderer.</returns>
    public static ShaderPipeline CreatePipeline(IRenderDevice renderer)
    {
        string vertexSource = Vertex(renderer.RenderTarget);
        string fragmentSource = Fragment(renderer.RenderTarget)
            .Replace("__MAX_LIGHTS__", SceneLighting.GetMaxLights(renderer).ToString(System.Globalization.CultureInfo.InvariantCulture));
        ShaderLanguage language = renderer.RenderTarget == RenderTarget.OpenGL ? ShaderLanguage.GLSL : ShaderLanguage.MSL;
        using var defaultVertex = new Shader(renderer, vertexSource, ShaderStage.Vertex, "vertex_main", language, false);
        using var defaultFragment = new Shader(renderer, fragmentSource, ShaderStage.Fragment, "fragment_main", language, false);

        var attributes = new List<VertexAttributeDescription>
        {
            new VertexAttributeDescription // Atribute 0 (position, float3)
            {
                AttributeIndex = 0,
                Format = VertexFormat.Float3,
                Offset = 0,
                BufferSlot = 0
            },
            new VertexAttributeDescription // Attribute 1 (texCoord, float2)
            {
                AttributeIndex = 1,
                Format = VertexFormat.Float2,
                Offset = 3 * sizeof(float),
                BufferSlot = 0
            },
            new VertexAttributeDescription // Attribute 2 (normal, float3)
            {
                AttributeIndex = 2,
                Format = VertexFormat.Float3,
                Offset = 5 * sizeof(float),
                BufferSlot = 0
            }
        };

        var vertexLayout = new VertexLayoutDescription
        {
            Buffers = [
                new VertexBufferLayoutDescription // Buffer 0
                {
                    BufferIndex = 0,
                    Stride = 8 * sizeof(float),
                    StepFunction = StepMode.PerVertex
                }
            ],
            Attributes = attributes.ToArray()
        };
        return new ShaderPipeline(defaultVertex, defaultFragment, vertexLayout, autoDisposeSource: true);
    }
}