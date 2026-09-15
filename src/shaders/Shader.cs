using System;

namespace odl3d;

/// <summary>
/// Represents a shader program used by the active renderer to encapsulate the state of vertex and fragment shaders for rendering. The Shader class provides methods for creating, compiling, and managing shader programs, allowing developers to define custom rendering behavior through programmable shaders.
/// </summary>
public class Shader
{
    /// <summary>
    /// The renderer-backed shader program handle for this shader; contains the state of the vertex and fragment shaders. The shader program is used to encapsulate the shader configuration and is managed by the renderer for efficient rendering.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// Indicates whether the shader has been disposed; used to prevent double disposal and ensure proper resource management. Once disposed, the shader handle is no longer valid and should not be used for rendering.
    /// </summary>
    protected IRendererOld Renderer => RenderFactory.Renderer;

    /// <summary>
    /// Indicates whether the shader has been disposed; used to prevent double disposal and ensure proper resource management. Once disposed, the shader handle is no longer valid and should not be used for rendering.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Shader"/> class, creating a new shader program with the specified source code and shader type. The shader is created using the renderer's <see cref="IRendererOld.CreateShader"/> method, and its handle is stored in the <see cref="Handle"/> property. The shader is compiled using the renderer's <see cref="IRendererOld.CompileShader"/> method, and if compilation fails, a <see cref="ShaderException"/> is thrown with the compilation log.
    /// </summary>
    /// <param name="source">The source code for the shader.</param>
    /// <param name="type">The type of the shader.</param>
    /// <exception cref="ShaderException">Thrown when shader compilation fails.</exception>
    public Shader(string source, ShaderType type)
    {
        Handle = Renderer.CreateShader(type);
        Renderer.SetShaderSource(this, source);
        bool success = Renderer.CompileShader(this);
        if (!success)
        {
            string log = Renderer.GetShaderLog(this);
            throw new ShaderException($"Shader compilation failed ({type}): {log}");
        }
    }

    /// <summary>
    /// Disposes of the shader, releasing its resources and marking it as disposed. After disposing, the shader handle is no longer valid and should not be used for rendering. The shader is deleted using the renderer's <see cref="IRendererOld.DeleteShader"/> method, and the <see cref="Disposed"/> property is set to true to indicate that the shader has been disposed.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Renderer.DeleteShader(this);
        Disposed = true;
    }
}