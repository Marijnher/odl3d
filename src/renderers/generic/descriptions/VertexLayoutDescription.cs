using System;
using System.Collections.Generic;

namespace odl3d.Renderer;

public sealed record VertexLayoutDescription
{
    public required VertexBufferLayoutDescription[] Buffers { get; init; }
    public required VertexAttributeDescription[] Attributes { get; init; }
}