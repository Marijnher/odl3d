using System;

namespace odl3d.Renderer;

public sealed record BufferDescription
{
    public required nuint Size { get; init; }
    public required BufferUsage Usage { get; init; }
    public BufferAccess Access { get; init; } = BufferAccess.GpuOnly;
}