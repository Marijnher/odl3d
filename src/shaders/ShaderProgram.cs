using System;
using System.Text;

namespace odl3d;

/// <summary>
/// A shader program consisting of a vertex shader and a fragment shader, compiled and linked into a single renderer program. The ShaderProgram class provides methods to compile the shaders from source code, link them into a program, set uniform variables, and use the program for rendering. It also implements IDisposable to allow for proper cleanup of renderer resources when the shader is no longer needed.
/// </summary>
public class ShaderProgram : IDisposable
{
    /// <summary>
    /// The renderer handle of the shader program; 0 if not yet created. The handle is assigned when the shader is compiled and linked, and it can be used to bind the program for rendering or to set uniform variables. The handle should be deleted when the shader is disposed to free renderer resources.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// The renderer instance used to create and manage this shader program. The Renderer property provides access to the active renderer, allowing the ShaderProgram to call renderer methods for compiling shaders, linking programs, setting uniforms, and managing resources. This property is used internally by the ShaderProgram class to interact with the rendering backend.
    /// </summary>
    protected IRenderer Renderer => RenderFactory.Renderer;

    /// <summary>
    /// Indicates whether this shader has been disposed and its resources released. After disposing, the shader should not be used again. The Disposed property is set to true when Dispose() is called, and it can be checked to prevent multiple disposals or usage of a disposed shader.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Creates a new ShaderProgram by compiling the given vertex and fragment shader source code, linking them into a shader program, and checking for compilation and linking errors. If any errors occur during compilation or linking, an exception is thrown with the error log. The resulting shader program can be used for rendering by calling Use() and setting uniform variables as needed.
    /// </summary>
    /// <param name="vertexSource">The source code of the vertex shader.</param>
    /// <param name="fragmentSource">The source code of the fragment shader.</param>
    /// <exception cref="ShaderException">Thrown if the vertex or fragment shader fails to compile, or if the shader program fails to link.</exception>
    public ShaderProgram(string vertexSource, string fragmentSource) :
        this(new Shader(vertexSource, ShaderType.Vertex), new Shader(fragmentSource, ShaderType.Fragment), true) { }

    /// <summary>
    /// Creates a new ShaderProgram by linking the given vertex and fragment Shader objects into a shader program. The shaders are attached to the program, linked, and checked for linking errors. If any errors occur during linking, an exception is thrown with the error log. Optionally, the source shaders can be automatically disposed after linking to free resources.
    /// </summary>
    /// <param name="vertexShader">The vertex shader to link.</param>
    /// <param name="fragmentShader">The fragment shader to link.</param>
    /// <param name="autoDisposeSource">Indicates whether to automatically dispose the source shaders after linking.</param>
    /// <exception cref="ShaderException">Thrown if the shader program fails to link.</exception>
    public ShaderProgram(Shader vertexShader, Shader fragmentShader, bool autoDisposeSource = true)
    {
        Handle = Renderer.CreateShaderProgram();
        Renderer.AttachShader(this, vertexShader);
        Renderer.AttachShader(this, fragmentShader);
        bool success = Renderer.LinkShaderProgram(this);

        if (!success)
        {
            string log = Renderer.GetShaderProgramLog(this);
            throw new ShaderException("Shader program failed to link: " + log);
        }

        if (autoDisposeSource)
        {
            vertexShader.Dispose();
            fragmentShader.Dispose();
        }
    }

    ~ShaderProgram()
    {
        if (!Disposed) Console.WriteLine("Warning: ShaderProgram was not disposed before being finalized. This may cause a renderer resource leak.");
    }

    /// <summary>
    /// Binds the shader program for use in rendering. After calling this method, subsequent draw calls will use this shader program until another shader is bound or the program is unbound. This method should be called before setting uniform variables or drawing objects that require this shader.
    /// </summary>
    public void Use(IRenderCommandEncoder commands) => commands.BindPipeline(this);

    /// <summary>
    /// Disposes of the shader, releasing its renderer resources. After calling this method, the shader should not be used again. If the shader has already been disposed, this method does nothing. This method should be called when the shader is no longer needed to free GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Renderer.DeleteShaderProgram(this);
        Disposed = true;
    }
}