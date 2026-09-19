using System;
using System.Numerics;
using System.Text;
using odl3d.Renderer;

namespace odl3d;

/// <summary>
/// A shader program consisting of a vertex shader and a fragment shader, compiled and linked into a single renderer program. The ShaderProgram class provides methods to compile the shaders from source code, link them into a program, set uniform variables, and use the program for rendering. It also implements IDisposable to allow for proper cleanup of renderer resources when the shader is no longer needed.
/// </summary>
public class ShaderPipeline : IDisposable
{
    /// <summary>
    /// The renderer handle of the shader program; 0 if not yet created. The handle is assigned when the shader is compiled and linked, and it can be used to bind the program for rendering or to set uniform variables. The handle should be deleted when the shader is disposed to free renderer resources.
    /// </summary>
    public IRenderPipeline Pipeline { get; private set; }

    protected IRenderDevice Renderer => Window.Renderer;

    /// <summary>
    /// Indicates whether this shader has been disposed and its resources released. After disposing, the shader should not be used again. The Disposed property is set to true when Dispose() is called, and it can be checked to prevent multiple disposals or usage of a disposed shader.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Creates a new ShaderProgram by linking the given vertex and fragment Shader objects into a shader program. The shaders are attached to the program, linked, and checked for linking errors. If any errors occur during linking, an exception is thrown with the error log. Optionally, the source shaders can be automatically disposed after linking to free resources.
    /// </summary>
    /// <param name="vertexShader">The vertex shader to link.</param>
    /// <param name="fragmentShader">The fragment shader to link.</param>
    /// <param name="autoDisposeSource">Indicates whether to automatically dispose the source shaders after linking.</param>
    /// <exception cref="ShaderException">Thrown if the shader program fails to link.</exception>
    public ShaderPipeline(Shader vertexShader, Shader fragmentShader, VertexLayoutDescription vertexLayout, bool autoDisposeSource = true)
    {
        Pipeline = Renderer.CreateRenderPipeline(new RenderPipelineDescription
        {
            VertexShader = vertexShader.ShaderModule,
            FragmentShader = fragmentShader.ShaderModule,
            ColorFormat = TextureFormat.RGBA32Float,
            VertexLayout = vertexLayout
        });
        if (autoDisposeSource)
        {
            vertexShader.Dispose();
            fragmentShader.Dispose();
        }
    }

    ~ShaderPipeline()
    {
        if (!Disposed) Console.WriteLine("Warning: ShaderProgram was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Sets a 4x4 matrix uniform variable in the shader program. The matrix is provided as a System.Numerics.Matrix4x4, and it is converted to a float array in column-major order before being passed to the renderer. The uniform variable is identified by its name, and the shader program must be in use (bound) when this method is called.
    /// </summary>
    /// <param name="name">The name of the uniform variable in the shader program.</param>
    /// <param name="matrix">The 4x4 matrix value to set for the uniform variable.</param>
    public void SetMatrix(string name, Matrix4x4 matrix)
    {
        // int location = Renderer.GetUniformLocation(this, name);
        // Renderer.SetUniformMatrix(location, matrix);
    }

    /// <summary>
    /// Sets an integer uniform variable in the shader program. The uniform variable is identified by its name, and the shader program must be in use (bound) when this method is called. This method can be used to set values for sampler uniforms or other integer parameters in the shader.
    /// </summary>
    /// <param name="name">The name of the uniform variable in the shader program.</param>
    /// <param name="value">The integer value to set for the uniform variable.</param>
    public void SetInt(string name, int value)
    {
        // int location = Renderer.GetUniformLocation(this, name);
        // Renderer.SetUniformInt(location, value);
    }

    /// <summary>
    /// Sets an RGBA color uniform variable in the shader program.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="color">The color value to set.</param>
    public void SetColor(string name, Color color)
    {
        // int location = Renderer.GetUniformLocation(this, name);
        // Renderer.SetUniformColor(location, color);
    }

    /// <summary>
    /// Disposes of the shader, releasing its renderer resources. After calling this method, the shader should not be used again. If the shader has already been disposed, this method does nothing. This method should be called when the shader is no longer needed to free GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Pipeline.Dispose();
        Disposed = true;
    }

    public static ShaderPipeline CreateDefault()
    {
        var defaultVertex = new Shader(defaultVertexMetal, ShaderStage.Vertex, "vertex_main", ShaderLanguage.MSL, true);
        var defaultFragment = new Shader(defaultFragmentMetal, ShaderStage.Fragment, "fragment_main", ShaderLanguage.MSL, true);
        var vertexLayout = new VertexLayoutDescription
        {
            Buffers = [
                new VertexBufferLayoutDescription // Buffer 0
                {
                    BufferIndex = 0,
                    Stride = 5 * sizeof(float),
                    StepFunction = StepMode.PerVertex
                }
            ],
            Attributes = [
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
                }
            ]
        };
        return new ShaderPipeline(defaultVertex, defaultFragment, vertexLayout);
    }

    private static string defaultVertexMetal = @$"demo/shaders/msl/simple_vertex.metal";

    private static string defaultFragmentMetal = @$"demo/shaders/msl/simple_fragment.metal";
}