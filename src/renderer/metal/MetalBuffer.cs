using System;

namespace odl3d.Renderer.MetalAdapter;

/// <summary>
/// Represents a generic buffer object for the Metal rendering backend, providing functionality for managing GPU memory and data updates.
/// </summary>
internal abstract class MetalBuffer
{
    /// <summary>
    /// Gets the size of the buffer in bytes.
    /// </summary>
    public int Size { get; protected set; }
    
    /// <summary>
    /// Gets the usage pattern of the buffer, indicating how it will be used in the rendering pipeline.
    /// </summary>
    public BufferUsage Usage { get; protected set; }

    /// <summary>
    /// Gets the usage hint of the buffer, providing additional information about how the buffer will be accessed and updated.
    /// </summary>
    public BufferHint Hint { get; protected set; }
    
    /// <summary>
    /// Gets a value indicating whether the buffer has been disposed.
    /// </summary>
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Gets the underlying Metal buffer object.
    /// </summary>
    public abstract Metal.Buffer Buffer { get; }

    /// <summary>
    /// Disposes the buffer, releasing its underlying Metal resources.
    /// </summary>
    public void Dispose()
    {
        if (Disposed) return;
         Buffer.Dispose();
         Disposed = true;
    }
}

/// <summary>
/// Represents a generic typed buffer object for the Metal rendering backend, providing functionality for managing GPU memory and data updates for a specific data type.
/// </summary>
/// <typeparam name="T">The type of data stored in the buffer.</typeparam>
internal class MetalBuffer<T> : MetalBuffer, IBuffer<T> where T : unmanaged
{
    /// <summary>
    /// Gets the underlying Metal buffer object for the typed buffer.
    /// </summary>
    public override Metal.Buffer Buffer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MetalBuffer{T}"/> class with the specified device, buffer description, and optional initial data.
    /// </summary>
    /// <param name="device">The Metal device used to create the buffer.</param>
    /// <param name="description">The description of the buffer, including its size, usage, and hint.</param>
    /// <param name="initialData">Optional initial data to populate the buffer with.</param>
    public MetalBuffer(Metal.Device device, BufferDescription description, T[]? initialData = null)
    {
        Size = description.Size;
        Usage = description.Usage;
        Hint = description.Hint;
        if (initialData != null && initialData.Length != Size) throw new RenderException("Provided data is not the same size as the buffer.");
        Buffer = device.CreateBuffer(initialData ?? new T[description.Size]);
    }
    
    /// <summary>
    /// Sets the data of the buffer, replacing its current contents with the specified data array.
    /// </summary>
    /// <param name="data">The data array to set in the buffer.</param>
    public void SetData(T[] data)
    {
        if (data.Length != Size) throw new RenderException("Provided data is not the same size as the buffer.");
        Buffer.Update(data, 0, data.Length);
    }

    /// <summary>
    /// Sets a portion of the buffer's data, replacing its current contents starting at the specified offset with the specified data array.
    /// </summary>
    /// <param name="data">The data array to set in the buffer.</param>
    /// <param name="offset">The offset in the buffer at which to start updating data.</param>
    /// <param name="count">The number of elements to update in the buffer.</param>
    public void SetData(T[] data, int offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > Size) throw new RenderException("Invalid offset or count for buffer update.");
        Buffer.Update(data, offset, count);
    }
}