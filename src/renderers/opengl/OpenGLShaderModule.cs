using System;
using System.Collections.Generic;

namespace odl3d.Renderer.OpenGLAdapter;

public class OpenGLShaderModule : IShaderModule
{
    public uint Handle { get; }
    public List<(string Name, int Slot)> UniformBlocks { get; } = new();

    public ShaderStage Stage { get; }
    public string Source { get; }
    public bool SourceAsFilename { get; }
    public string EntryPoint { get; }
    public ShaderLanguage ShaderLanguage { get; }
    public bool Disposed { get; private set; }

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
    
    public void Dispose()
    {
        if (Disposed) return;
        GL.glDeleteShader(Handle);
        Disposed = true;
    }
}