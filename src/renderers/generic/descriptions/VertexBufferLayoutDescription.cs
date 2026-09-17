using System;

namespace odl3d.Renderer;

public sealed record VertexBufferLayoutDescription
{
    public required uint BufferIndex { get; init; }
    public required uint Stride { get; init; }
    public StepMode StepFunction { get; init; } = StepMode.PerVertex;
}