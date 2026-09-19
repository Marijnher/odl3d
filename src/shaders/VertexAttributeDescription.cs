using System;

namespace odl3d;

public sealed record VertexAttributeDescription
{
    public required uint AttributeIndex { get; init; }
    public required VertexFormat Format { get; init; }
    public required int Offset { get; init; }
    public required uint BufferSlot { get; init; }
}