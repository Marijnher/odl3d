using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a buffer, including its size, usage, and hint for the rendering pipeline.
/// </summary>
public sealed record BufferDescription
{
    /// <summary>
    /// The size of the buffer in <typeparamref name="T"/> units.
    /// </summary>
    public required int Size { get; init; }

    /// <summary>
    /// The intended usage of the buffer (e.g., vertex buffer, index buffer).
    /// </summary>
    public required BufferUsage Usage { get; init; }

    /// <summary>
    /// A hint for the rendering pipeline on how the buffer will be used.
    /// </summary>
    public BufferHint Hint { get; init; }
}