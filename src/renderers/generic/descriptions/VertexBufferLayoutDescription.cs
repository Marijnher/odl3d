using System;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a vertex buffer layout, including its buffer index, stride, and step function.
/// </summary>
public sealed record VertexBufferLayoutDescription
{
    /// <summary>
    /// The index of the vertex buffer.
    /// </summary>
    public required uint BufferIndex { get; init; }

    /// <summary>
    /// The stride of the vertex buffer, specifying the byte offset between consecutive vertices.
    /// </summary>
    public required uint Stride { get; init; }
    
    /// <summary>
    /// The step function of the vertex buffer, determining how vertex data is advanced.
    /// </summary>
    public StepMode StepFunction { get; init; } = StepMode.PerVertex;
}