using System;

namespace odl3d.Renderer;

public sealed record VertexAttributeDescription
{
    public required int Location { get; init; }
    public required int BufferSlot { get; init; }
    public required VertexFormat Format { get; init; }
    public required int Offset { get; init; }
}