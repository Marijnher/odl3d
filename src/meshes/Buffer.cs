using System;

namespace odl3d;

/// <summary>
/// Represents a buffer object, which encapsulates the state of vertex or index data for rendering. The buffer object is used to store vertex or index data in GPU memory, allowing for efficient rendering of meshes. The buffer can be bound to a specific target (e.g., array buffer or element buffer) and can be filled with data using the SetData methods. The buffer is disposed when no longer needed, releasing its resources in GPU memory.
/// </summary>
public class Buffer
{
    /// <summary>
    /// The renderer instance used to create and manage this buffer. The Renderer property provides access to the active renderer, allowing the Buffer to call renderer methods for creating buffers, setting data, binding buffers, and managing resources. This property is used internally by the Buffer class to interact with the rendering backend.
    /// </summary>
    protected IRenderer Renderer => RenderFactory.Renderer;

    /// <summary>
    /// The renderer-backed buffer handle for this buffer; contains the vertex or index data stored in GPU memory. The handle is used to bind the buffer to a specific target (e.g., array buffer or element buffer) and to set its data using the SetData methods. The handle is generated when the buffer is created and is released when the buffer is disposed.
    /// </summary>
    public uint Handle { get; private set; }

    /// <summary>
    /// The target to which the buffer is bound (e.g., array buffer or element buffer). The target determines how the buffer is used in rendering operations, such as storing vertex data for drawing or storing index data for indexed drawing. The target is specified when the buffer is created and cannot be changed after creation.
    /// </summary>
    public BufferTarget BufferTarget { get; private set; }

    /// <summary>
    /// Indicates whether the buffer has been disposed; used to prevent double disposal and ensure proper resource management. Once disposed, the buffer handle is no longer valid and should not be used for rendering.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Buffer"/> class, creating a new buffer object. The buffer is created using the renderer's <see cref="IRenderer.CreateBuffer"/> method, and its handle is stored in the <see cref="Handle"/> property. The buffer target is specified as a parameter and determines how the buffer is used in rendering operations. The buffer can be filled with data using the SetData methods and is disposed when no longer needed.
    /// </summary>
    /// <param name="bufferTarget"></param>
    public Buffer(BufferTarget bufferTarget)
    {
        Handle = Renderer.CreateBuffer();
        BufferTarget = bufferTarget;
    }

    /// <summary>
    /// Sets the data for the buffer object, uploading the specified float array to GPU memory. The data is bound to the buffer's target (e.g., array buffer or element buffer) and is uploaded using the renderer's <see cref="IRenderer.SetBufferData"/> method. The buffer hint specifies how the data will be used (e.g., static, dynamic, or stream), allowing the GPU to optimize its storage and access patterns for efficient rendering.
    /// </summary>
    /// <param name="data">The float array containing the data to upload.</param>
    /// <param name="hint">The buffer hint specifying how the data will be used.</param>
    public void SetData(float[] data, BufferHint hint = BufferHint.Static)
    {
        Renderer.BindBuffer(BufferTarget, Handle);
        Renderer.SetBufferData(BufferTarget, Handle, data, hint);
    }

    /// <summary>
    /// Sets the data for the buffer object, uploading the specified uint array to GPU memory. The data is bound to the buffer's target (e.g., array buffer or element buffer) and is uploaded using the renderer's <see cref="IRenderer.SetBufferData"/> method. The buffer hint specifies how the data will be used (e.g., static, dynamic, or stream), allowing the GPU to optimize its storage and access patterns for efficient rendering.
    /// </summary>
    /// <param name="data">The uint array containing the data to upload.</param>
    /// <param name="hint">The buffer hint specifying how the data will be used.</param>
    public void SetData(uint[] data, BufferHint hint = BufferHint.Static)
    {
        Renderer.BindBuffer(BufferTarget, Handle);
        Renderer.SetBufferData(BufferTarget, Handle, data, hint);
    }

    /// <summary>
    /// Disposes the buffer object, releasing its resources in GPU memory. The buffer is deleted using the renderer's <see cref="IRenderer.DeleteBuffer"/> method, and its handle is no longer valid after disposal. The <see cref="Disposed"/> property is set to true to indicate that the buffer has been disposed and should not be used for rendering.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        Renderer.DeleteBuffer(Handle);
        Disposed = true;
    }
}