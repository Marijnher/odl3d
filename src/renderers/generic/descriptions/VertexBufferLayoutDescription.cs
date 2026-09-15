using System;

namespace odl3d.Renderer;

public sealed record VertexBufferLayoutDescription
{
    public required int Slot { get; init; }
    public required int Stride { get; init; }
    public StepMode StepMode { get; init; } = StepMode.PerVertex;
}