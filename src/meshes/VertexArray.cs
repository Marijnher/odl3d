using System;
using System.Collections.Generic;
using System.Linq;

namespace odl3d;

/// <summary>
/// Represents a vertex array object (VAO) used by the active renderer to encapsulate the state of vertex attributes and buffer bindings for rendering. The VAO stores the configuration needed to efficiently render meshes.
/// </summary>
public class VertexArray : IDisposable
{
    /// <summary>
    /// The renderer instance used to create and manage this vertex array. The Renderer property provides access to the active renderer, allowing the VertexArray to call renderer methods for creating vertex arrays, binding them, enabling vertex attributes, and managing resources. This property is used internally by the VertexArray class to interact with the rendering backend.
    /// </summary>
    protected IRenderer Renderer => RenderFactory.Renderer;

    /// <summary>
    /// The renderer-backed vertex array object (VAO) handle for this vertex array; contains the state of the vertex attributes and buffer bindings. The VAO is used to encapsulate the vertex attribute configuration and buffer bindings for rendering.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// Indicates whether the vertex array has been disposed; used to prevent double disposal and ensure proper resource management. Once disposed, the VAO handle is no longer valid and should not be used for rendering.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Queues attributes to automatically calculate stride when unbinding.
    /// </summary>
    private List<int> attributes = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VertexArray"/> class, creating a new vertex array object (VAO). The VAO is created using the renderer's <see cref="IRenderer.CreateVertexArray"/> method, and its handle is stored in the <see cref="Handle"/> property. The VAO is used to encapsulate the state of vertex attributes and buffer bindings for efficient rendering of meshes.
    /// </summary>
    public VertexArray()
    {
        Handle = Renderer.CreateVertexArray();
    }

    /// <summary>
    /// Binds the vertex array object (VAO) for rendering, making it the current VAO. The VAO is bound using the renderer's <see cref="IRenderer.BindVertexArray"/> method, allowing subsequent rendering operations to use the state of the VAO for vertex attribute configuration and buffer bindings.
    /// </summary>
    public void Bind() => Renderer.BindVertexArray(Handle);

    /// <summary>
    /// Adds a vertex attribute to the vertex array object (VAO) for automatic stride calculation when unbinding. The attribute length is specified in terms of the number of components (e.g., 3 for a vec3 position attribute). The attribute is added to the internal list of attributes, which is used to calculate the stride and offset for each attribute when unbinding the VAO.
    /// </summary>
    /// <param name="length">The length of the vertex attribute.</param>
    public void AddAttribute(int length)
    {
        attributes.Add(length);
    }

    /// <summary>
    /// Unbinds the vertex array object (VAO) and automatically calculates the stride and offset for each vertex attribute based on the queued attributes. The stride is calculated as the sum of the sizes of all attributes, and the offset for each attribute is calculated based on its position in the list of attributes. The VAO is unbound using the renderer's <see cref="IRenderer.BindVertexArray"/> method, and the internal list of attributes is cleared after unbinding.
    /// </summary>
    public void Unbind()
    {
        int stride = attributes.Sum() * sizeof(float);
        int offset = 0;
        for (int i = 0; i < attributes.Count; i++)
        {
            int length = attributes[i];
            Renderer.EnableVertexAttribute(i);
            Renderer.AddVertexAttribute(i, length, stride, offset);
            offset += length;
        }
        attributes.Clear();
        Renderer.BindVertexArray(0);
    }

    /// <summary>
    /// Disposes of the vertex array object (VAO) and releases its resources. The VAO is deleted using the renderer's <see cref="IRenderer.DeleteVertexArray"/> method, and the <see cref="Disposed"/> property is set to true to indicate that the VAO has been disposed. Once disposed, the VAO handle is no longer valid and should not be used for rendering.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Renderer.DeleteVertexArray(Handle);
        Disposed = true;
    }
}