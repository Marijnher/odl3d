using System;
using System.Collections.Generic;

namespace odl3d.Renderer;

/// <summary>
/// Represents the description of a vertex layout, including its vertex buffer layouts and vertex attributes.
/// </summary>
public sealed record VertexLayoutDescription
{
    /// <summary>
    /// The array of vertex buffer layout descriptions that define the structure of the vertex buffers.
    /// </summary>
    public required VertexBufferLayoutDescription[] Buffers { get; init; }
    
    /// <summary>
    /// The array of vertex attribute descriptions that define the attributes of the vertices.
    /// </summary>
    public required VertexAttributeDescription[] Attributes { get; init; }
}