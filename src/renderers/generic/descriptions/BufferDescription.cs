using System;

namespace odl3d.Renderer;

public sealed record BufferDescription
{
    public required int Size { get; init; }
    public required BufferUsage Usage { get; init; }
}