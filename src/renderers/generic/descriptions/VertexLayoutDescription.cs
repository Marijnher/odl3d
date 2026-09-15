using System;
using System.Collections.Generic;

namespace odl3d.Renderer;

public sealed record VertexLayoutDescription
{
    public required IReadOnlyList<VertexBufferLayoutDescription> Buffers { get; init; }
    public required IReadOnlyList<VertexAttributeDescription> Attributes { get; init; }
}