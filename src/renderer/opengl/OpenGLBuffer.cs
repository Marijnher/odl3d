using System;

namespace odl3d.Renderer.OpenGLAdapter;

/// <summary>
/// Represents an OpenGL buffer of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
internal class OpenGLBuffer<T> : IBuffer<T> where T : unmanaged
{
    /// <summary>
    /// Gets the OpenGL handle for the buffer.
    /// </summary>
    public uint Handle { get; }

    private OpenGLRenderDevice Device;

    /// <summary>
    /// Gets the size of the buffer in bytes.
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Gets the usage pattern of the buffer.
    /// </summary>
    public BufferUsage Usage { get; }

    /// <summary>
    /// Gets the hint for the buffer.
    /// </summary>
    public BufferHint Hint { get; }

    /// <summary>
    /// Gets a value indicating whether the buffer has been disposed.
    /// </summary>
    public bool Disposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGLBuffer{T}"/> class.
    /// </summary>
    /// <param name="description">The description of the buffer.</param>
    /// <param name="initialData">The initial data to populate the buffer with, if any.</param>
    public OpenGLBuffer(OpenGLRenderDevice device, BufferDescription description, T[]? initialData = null)
    {
        Device = device;
        Size = description.Size;
        Usage = description.Usage;
        Hint = description.Hint;
        GL.glGenBuffers(1, out uint handle);
        Handle = handle;
        if (initialData != null) SetData(initialData);
    }

    private uint GetHintType() => Hint switch
    {
        BufferHint.Static => GL.GL_STATIC_DRAW,
        BufferHint.Dynamic => GL.GL_DYNAMIC_DRAW,
        BufferHint.Stream => GL.GL_STREAM_DRAW,
        _ => throw new RenderException("Unsupported buffer hint.")
    };

    /// <summary>
    /// Sets the data for the buffer.
    /// </summary>
    /// <param name="data">The data to set for the buffer.</param>
    public void SetData(T[] data)
    {
        SetData(data, 0, data.Length);
    }

    /// <summary>
    /// Sets a portion of the data for the buffer.
    /// </summary>
    /// <param name="data">The data to set for the buffer.</param>
    /// <param name="offset">The offset in the data array to start from.</param>
    /// <param name="count">The number of elements to set in the buffer.</param>
    public unsafe void SetData(T[] data, int offset, int count)
    {
        GL.glBindBuffer(GL.GL_COPY_WRITE_BUFFER, Handle);
        fixed (T* dataPtr = data) GL.glBufferData(GL.GL_COPY_WRITE_BUFFER, count * sizeof(T), (nint) (dataPtr + offset), GetHintType());
    }

    /// <summary>
    /// Disposes the buffer and releases its resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
        uint handle = Handle;
        // Release buffer from the device's vertex state (VAO) cache before deleting it.
        Device.VertexState!.OnBufferDeleted(handle);
        GL.glDeleteBuffers(1, ref handle);
        Disposed = true;
    }
}