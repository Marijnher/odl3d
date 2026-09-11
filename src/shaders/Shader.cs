using System;
using System.Numerics;
using System.Text;

namespace odl3d;

/// <summary>
/// A shader program consisting of a vertex shader and a fragment shader, compiled and linked into a single OpenGL program. The Shader class provides methods to compile the shaders from source code, link them into a program, set uniform variables, and use the program for rendering. It also implements IDisposable to allow for proper cleanup of OpenGL resources when the shader is no longer needed.
/// </summary>
public class Shader : IDisposable
{
    /// <summary>
    /// The OpenGL handle of the shader program; 0 if not yet created. The handle is assigned when the shader is compiled and linked, and it can be used to bind the program for rendering or to set uniform variables. The handle should be deleted when the shader is disposed to free OpenGL resources.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// The renderer responsible for executing the shader program. This property is set when the shader is created and is used to bind the shader for rendering and to set uniform variables.
    /// </summary>
    public IRenderer Renderer { get; private set; }

    /// <summary>
    /// Indicates whether this shader has been disposed and its resources released. After disposing, the shader should not be used again. The Disposed property is set to true when Dispose() is called, and it can be checked to prevent multiple disposals or usage of a disposed shader.
    /// </summary>
    public bool Disposed { get; private set; } = false;

    /// <summary>
    /// Invoked when this shader is disposed. Subscribers can use this event to perform cleanup or other actions when the shader is no longer needed.
    /// </summary>
    public event Action? OnDisposed;

    /// <summary>
    /// Creates a new Shader by compiling the given vertex and fragment shader source code, linking them into a shader program, and checking for compilation and linking errors. If any errors occur during compilation or linking, an exception is thrown with the error log. The resulting shader program can be used for rendering by calling Use() and setting uniform variables as needed.
    /// </summary>
    /// <param name="vertexSource">The source code of the vertex shader.</param>
    /// <param name="fragmentSource">The source code of the fragment shader.</param>
    /// <exception cref="ShaderException">Thrown if the vertex or fragment shader fails to compile, or if the shader program fails to link.</exception>
    public Shader(IRenderer renderer,string vertexSource, string fragmentSource)
    {
        Renderer = renderer;

        uint vertex = Compile(ShaderType.Vertex, vertexSource);
        uint fragment = Compile(ShaderType.Fragment, fragmentSource);

        Handle = Renderer.CreateShaderProgram();
        Renderer.AttachShader(Handle, vertex);
        Renderer.AttachShader(Handle, fragment);
        bool success = Renderer.LinkShaderProgram(Handle);

        if (!success)
        {
            string log = Renderer.GetShaderProgramLog(Handle);
            throw new ShaderException("Shader program failed to link: " + log);
        }

        Renderer.DeleteShader(vertex);
        Renderer.DeleteShader(fragment);
    }

    ~Shader()
    {
        if (!Disposed) Console.WriteLine("Warning: Shader was not disposed before being finalized. This may cause a GL resource leak.");
    }

    /// <summary>
    /// Compiles a shader of the given type (vertex or fragment) from the provided source code. If compilation fails, an exception is thrown with the error log. The compiled shader handle is returned for linking into a shader program.
    /// </summary>
    /// <param name="type">The type of shader to compile (ShaderType.Vertex or ShaderType.Fragment).</param>
    /// <param name="source">The source code of the shader.</param>
    /// <returns>The handle of the compiled shader.</returns>
    /// <exception cref="ShaderException">Thrown if the shader fails to compile.</exception>
    private uint Compile(ShaderType type, string source)
    {
        uint shader = Renderer.CreateShader(type);
        Renderer.SetShaderSource(shader, source);
        bool success = Renderer.CompileShader(shader);
        if (!success)
        {
            string log = Renderer.GetShaderLog(shader);
            throw new ShaderException("Shader failed to compile: " + log);
        }
        return shader;
    }

    /// <summary>
    /// Binds the shader program for use in rendering. After calling this method, subsequent draw calls will use this shader program until another shader is bound or the program is unbound. This method should be called before setting uniform variables or drawing objects that require this shader.
    /// </summary>
    public void Use() => Renderer.UseShaderProgram(Handle);

    /// <summary>
    /// Sets a 4x4 matrix uniform variable in the shader program. The matrix is provided as a System.Numerics.Matrix4x4, and it is converted to a float array in column-major order before being passed to the renderer. The uniform variable is identified by its name, and the shader program must be in use (bound) when this method is called.
    /// </summary>
    /// <param name="name">The name of the uniform variable in the shader program.</param>
    /// <param name="matrix">The 4x4 matrix value to set for the uniform variable.</param>
    public void SetMatrix(string name, Matrix4x4 matrix)
    {
        int location = Renderer.GetUniformLocation(Handle, name);
        Renderer.SetUniformMatrix(location, matrix);
    }

    /// <summary>
    /// Sets an integer uniform variable in the shader program. The uniform variable is identified by its name, and the shader program must be in use (bound) when this method is called. This method can be used to set values for sampler uniforms or other integer parameters in the shader.
    /// </summary>
    /// <param name="name">The name of the uniform variable in the shader program.</param>
    /// <param name="value">The integer value to set for the uniform variable.</param>
    public void SetInt(string name, int value)
    {
        int location = Renderer.GetUniformLocation(Handle, name);
        Renderer.SetUniformInt(location, value);
    }

    /// <summary>
    /// Sets an RGBA color uniform variable in the shader program.
    /// </summary>
    /// <param name="name">The name of the uniform variable.</param>
    /// <param name="color">The color value to set.</param>
    public void SetColor(string name, Color color)
    {
        int location = Renderer.GetUniformLocation(Handle, name);
        Renderer.SetUniformColor(location, color);
    }

    /// <summary>
    /// Disposes of the shader, releasing its OpenGL resources. After calling this method, the shader should not be used again. If the shader has already been disposed, this method does nothing. This method should be called when the shader is no longer needed to free GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Renderer.DeleteShaderProgram(Handle);
        Disposed = true;
        OnDisposed?.Invoke();
    }
}

/// <summary>
/// An exception that is thrown when a shader fails to compile or link. This exception can be used to indicate errors in shader source code or issues with the OpenGL shader compilation and linking process. The message of the exception typically contains the error log from the shader compiler or linker, providing details about what went wrong.
/// </summary>
class ShaderException : Exception
{
    /// <summary>
    /// Creates a new ShaderException with the specified error message. The message should describe the reason for the exception, such as a compilation or linking error in the shader program. This constructor can be used to create an instance of ShaderException when a shader fails to compile or link, allowing the application to handle the error appropriately.
    /// </summary>
    /// <param name="message">The error message describing the reason for the exception.</param>
    public ShaderException(string message) : base(message) { }
}