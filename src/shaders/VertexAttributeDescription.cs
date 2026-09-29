using System;

namespace odl3d;

/// <summary>
/// Describes a single vertex attribute within a vertex layout, including its format, offset, and buffer slot.
/// </summary>
public sealed record VertexAttributeDescription
{
    /// <summary>
    /// The index of the vertex attribute within the vertex layout.
    /// </summary>
    public required uint AttributeIndex { get; init; }
    
    /// <summary>
    /// The format of the vertex attribute (e.g., float3, float2).
    /// </summary>
    public required VertexFormat Format { get; init; }

    /// <summary>
    /// The byte offset of the vertex attribute within the vertex buffer.
    /// </summary>
    public required int Offset { get; init; }

    /// <summary>
    /// The index of the vertex buffer slot that contains this attribute.
    /// </summary>
    public required uint BufferSlot { get; init; }
}