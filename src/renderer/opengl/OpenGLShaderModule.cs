using System;
using System.Collections.Generic;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL shader module, encapsulating the shader handle, stage, source, and related information.
/// </summary>
internal class OpenGLShaderModule : IShaderModule
{
    /// <summary>
    /// Gets the handle of the OpenGL shader.
    /// </summary>
    public uint Handle { get; }

    /// <summary>
    /// Gets the list of uniform blocks in the shader, along with their names and slots.
    /// </summary>
    public List<(string Name, int Slot)> UniformBlocks { get; } = new();

    /// <summary>
    /// Gets the stage of the shader (e.g., vertex, fragment, compute).
    /// </summary>
    public ShaderStage Stage { get; }

    /// <summary>
    /// Gets the source code of the shader.
    /// </summary>
    public string Source { get; }

    /// <summary>
    /// Gets a value indicating whether the source is treated as a filename.
    /// </summary>
    public bool SourceAsFilename { get; }

    /// <summary>
    /// Gets the entry point of the shader.
    /// </summary>
    public string EntryPoint { get; }

    /// <summary>
    /// Gets the shading language of the shader.
    /// </summary>
    public ShaderLanguage ShaderLanguage { get; }

    /// <summary>
    /// Gets a value indicating whether the shader module has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLShaderModule"/> class with the specified description.
    /// </summary>
    /// <param name="description">The description of the shader module to create.</param>
    /// <exception cref="RenderException">Thrown if the shader fails to compile or if an unsupported shader stage is specified.</exception>
    public OpenGLShaderModule(ShaderModuleDescription description)
    {
        Stage = description.Stage;
        Source = description.Source;
        SourceAsFilename = description.SourceAsFilename;
        EntryPoint = description.EntryPoint;
        ShaderLanguage = description.ShaderLanguage;
        if (SourceAsFilename) Source = System.IO.File.ReadAllText(Source);
        
        Handle = GL.glCreateShader(Stage switch
        {
            ShaderStage.Vertex => GL.GL_VERTEX_SHADER,
            ShaderStage.Fragment => GL.GL_FRAGMENT_SHADER,
            ShaderStage.Compute => GL.GL_COMPUTE_SHADER,
            _ => throw new RenderException($"Unsupported shader stage: {Stage}")
        });

        string shaderSource = Source;
        shaderSource = ShaderPreprocessor.Process(shaderSource, UniformBlocks);

        GL.glShaderSource(Handle, 1, [shaderSource], [shaderSource.Length]);
        GL.glCompileShader(Handle);
        GL.glGetShaderiv(Handle, GL.GL_COMPILE_STATUS, out int compileStatus);
        if (compileStatus == 0)
        {
            var infoLog = new byte[1024];
            GL.glGetShaderInfoLog(Handle, 1024, out int length, infoLog);
            GL.glDeleteShader(Handle);
            throw new RenderException($"Failed to compile shader: {System.Text.Encoding.UTF8.GetString(infoLog, 0, length)}");
        }
    }

    /// <summary>
    /// Disposes the shader module, releasing its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        GL.glDeleteShader(Handle);
        Disposed = true;
    }
}