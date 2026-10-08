using System;
using System.Collections.Generic;
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

    /// <summary>
    /// Gets or sets whether the shader pipeline should render in wireframe mode.
    /// </summary>
    public bool Wireframe
    {
        get => Pipeline.Wireframe;
        set => Pipeline.Wireframe = value;
    }

    /// <summary>
    /// Indicates whether this shader pipeline has been disposed and its resources released. After disposing, the shader pipeline should not be used again. The Disposed property is set to true when Dispose() is called, and it can be checked to prevent multiple disposals or usage of a disposed shader pipeline.
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
        if (!ReferenceEquals(vertexShader.Renderer, fragmentShader.Renderer))
            throw new ArgumentException("Both shaders must belong to the same renderer.");
        Pipeline = vertexShader.Renderer.CreateRenderPipeline(new RenderPipelineDescription
        {
            VertexShader = vertexShader.ShaderModule,
            FragmentShader = fragmentShader.ShaderModule,
            ColorFormat = TextureFormat.RGBA32Float,
            VertexLayout = vertexLayout,
            Blend = new BlendDescription()
            {
                Enabled = true,
                SourceColor = BlendFactor.SourceAlpha,
                DestinationColor = BlendFactor.OneMinusSourceAlpha,
                ColorOperation = BlendOperation.Add,
                SourceAlpha = BlendFactor.One,
                DestinationAlpha = BlendFactor.Zero,
                AlphaOperation = BlendOperation.Add
            },
            PrimitiveType = PrimitiveType.TriangleList
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
    /// Disposes of the shader, releasing its renderer resources. After calling this method, the shader should not be used again. If the shader has already been disposed, this method does nothing. This method should be called when the shader is no longer needed to free GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Pipeline.Dispose();
        Disposed = true;
    }
}