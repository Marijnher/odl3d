using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents a generic buffer resource on the GPU.
/// </summary>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
public interface IBuffer<T> : IGPUResource where T : unmanaged
{
    /// <summary>
    /// Gets the size of the buffer in bytes.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Gets the usage flags of the buffer.
    /// </summary>
    BufferUsage Usage { get; }

    /// <summary>
    /// Gets the hint for the buffer's intended usage pattern.
    /// </summary>
    BufferHint Hint { get; }

    /// <summary>
    /// Sets the data of the buffer from the provided array.
    /// </summary>
    /// <param name="data">The array containing the data to set in the buffer.</param>
    void SetData(T[] data);

    /// <summary>
    /// Sets a portion of the buffer's data from the provided array.
    /// </summary>
    /// <param name="data">The array containing the data to set in the buffer.</param>
    /// <param name="offset">The starting index in the array from which to copy data.</param>
    /// <param name="count">The number of elements to copy from the array into the buffer.</param>
    void SetData(T[] data, int offset, int count);
}