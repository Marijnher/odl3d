using System;

namespace odl3d.Renderer;

public sealed record TextureDescription
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required TextureFormat Format { get; init; }

    public int Depth { get; init; } = 1;
    public int MipmapLevels { get; init; } = 1;
    public int SampleCount { get; init; } = 1;

    public TextureUsage Usage { get; init; } = TextureUsage.Sampled;
}